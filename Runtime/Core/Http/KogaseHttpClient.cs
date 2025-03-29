using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Kogase.Core;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace Kogase.Core
{
    /// <summary>
    /// Client for executing HTTP requests
    /// </summary>
    public class KogaseHttpClient : IHttpClient
    {
        internal const string ApiKeyHeaderKey = "X-Kogase-API-Key";

        enum RequestState
        {
            INVALID = 0,
            RUNNING,
            PAUSED,
            RESUMED,
            STOPPED
        }

        public event Action<IHttpRequest> ServerErrorOccured;

        public event Action<IHttpRequest> NetworkErrorOccured;

        public event Func<string, Action<string>, bool> BearerAuthRejected;

        public event Action<string> UnauthorizedOccured;

        IDebugger logger;

        public KogaseHttpClient(IHttpRequestSender requestSender = null, IDebugger logger = null)
        {
            SetLogger(logger);
            if (requestSender != null)
            {
                Sender = requestSender;
            }
            else
            {
                var defaultScheduler = new WebRequestScheduler();
                var defaultSender = new HttpRequestSender(defaultScheduler);
                defaultScheduler.SetLogger(logger);

                var heartBeat = new HeartBeat();
                defaultSender.SetHeartBeat(heartBeat);
                Sender = defaultSender;
            }

            SetRetryParameters();
        }

        public void SetLogger(IDebugger newLogger)
        {
            logger = newLogger;
        }

        public void SetApiKey(string newApiKey)
        {
            apiKey = newApiKey;
        }

        public void AddAdditionalHeaderInfo(string headerKey, string headerValue)
        {
            additionalHeaderInfoDict[headerKey] = headerValue;
        }

        public void SetImplicitPathParams(IDictionary<string, string> pathParams)
        {
            foreach (var param in pathParams) pathParamsDict[param.Key] = param.Value;
        }

        public void ClearImplicitPathParams()
        {
            pathParamsDict.Clear();
        }

        public void ClearCookies()
        {
            Sender.ClearCookies(baseUri);
        }

        public void SetBaseUri(Uri uri)
        {
            baseUri = uri;
        }

        public void SetSender(IHttpRequestSender newSender)
        {
            if (newSender != null) Sender = newSender;
        }

        public void OnBearerAuthRejected(Action<string> callback)
        {
            PauseBearerAuthRequest();
            if (IsRequestingNewAccessToken()) return;

            if (BearerAuthRejected == null)
            {
                callback?.Invoke(null);
                return;
            }

            var isRequestAccessTokenStart = BearerAuthRejected.Invoke(accessToken, result =>
            {
                SetRequestingNewAccessToken(false);
                if (result != null) ResumeBearerAuthRequest();
                callback?.Invoke(result);
            });

            if (isRequestAccessTokenStart) SetRequestingNewAccessToken(true);
        }

        public void SetImplicitBearerAuth(string newAccessToken)
        {
            accessToken = newAccessToken;
        }

        public void SetRetryParameters(uint totalTimeoutMs = 60000, uint initialDelayMs = 1000, uint maxDelayMs = 30000,
            uint maxRetries = 3)
        {
            this.totalTimeoutMs = totalTimeoutMs;
            this.initialDelayMs = initialDelayMs;
            this.maxDelayMs = maxDelayMs;
            this.maxRetries = maxRetries;
        }

        public async Task<HttpSendResult> SendRequestAsync(IHttpRequest requestInput)
        {
            var requestUniqueIdentifier = Guid.NewGuid().ToString();
            var rand = new Random();
            var retryDelayMs = initialDelayMs;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var request = requestInput;

            ApplyImplicitAuthorization(request, out var applyAuthErr);
            if (applyAuthErr.Code != ErrorCode.NONE) return new HttpSendResult(null, applyAuthErr);

            ApplyImplicitPathParams(request);

            ApplyAdditionalData(request, additionalHeaderInfoDict, out var applyAdditionalDataErr);
            if (applyAdditionalDataErr.Code != ErrorCode.NONE) return new HttpSendResult(null, applyAdditionalDataErr);

            if (!TryResolveUri(request))
                return new HttpSendResult(null,
                    new Error(ErrorCode.INVALID_REQUEST, "Invalid uri format: ", request.Url));

            IHttpResponse httpResponse;
            Error error;

            RequestState state;
            if (IsBearerAuthRequestPaused() && request.AuthType == HttpAuthType.BEARER)
                state = RequestState.PAUSED;
            else
                state = RequestState.RUNNING;

            var retryTimes = 0;
            do
            {
                if (state == RequestState.PAUSED)
                {
                    if (IsBearerAuthRequestPaused())
                    {
                        OnBearerAuthRejected(null);
                        while (IsBearerAuthRequestPaused() && IsRequestingNewAccessToken())
                            await HttpHelper.HttpDelayOneFrame;

                        if (IsBearerAuthRequestPaused())
                            logger?.LogWarning("Failed retrieving new access token, resuming");
                    }

                    state = RequestState.RESUMED;
                    stopwatch.Restart();
                    if (IsBearerAuthRequestPaused())
                    {
                        request.Headers.Remove("Authorization");
                        ApplyImplicitAuthorization(request, out var applyErr);
                        if (applyErr.Code != ErrorCode.NONE) return new HttpSendResult(null, applyErr);
                    }
                }

                var timeoutMs = (int)(totalTimeoutMs - stopwatch.ElapsedMilliseconds);

                uint delayMs = 0;
                if (retryTimes > 0)
                {
                    logger?.LogWarning($"Send request failed, Retrying {retryTimes}/{maxRetries}");
                    request.Priority = HttpHelper.HttpRequestDefaultPriority - 1;

                    var jitterMs = rand.Next((int)(-retryDelayMs / 4), (int)(retryDelayMs / 4));
                    delayMs = (uint)(retryDelayMs + jitterMs);

                    retryDelayMs = Math.Min(retryDelayMs * 2, maxDelayMs);
                }

                HttpSendResult? sendResult = null;

                request.Id = $"{requestUniqueIdentifier}-{retryTimes}";
                Sender.AddTask(request, OnSendRequestComplete, timeoutMs, delayMs);
                while (sendResult == null) await HttpHelper.HttpDelayOneFrame;
                httpResponse = sendResult.Value.CallbackResponse;
                error = sendResult.Value.CallbackError;

                void OnSendRequestComplete(HttpSendResult result)
                {
                    sendResult = result;
                }

                if (httpResponse == null) return new HttpSendResult(null, error);

                var requireToRetry =
                    CheckRequireRetry(request, httpResponse, error, NetworkErrorOccured, ServerErrorOccured);

                if (requireToRetry)
                {
                    retryTimes++;
                    if (retryTimes > maxRetries) return new HttpSendResult(httpResponse, error);
                    continue;
                }

                if (httpResponse.Code == (long)HttpStatusCode.Unauthorized)
                {
                    if (state == RequestState.RESUMED || request.AuthType != HttpAuthType.BEARER)
                    {
                        UnauthorizedOccured?.Invoke(accessToken);
                        return new HttpSendResult(httpResponse, error);
                    }

                    state = RequestState.PAUSED;
                    OnBearerAuthRejected(null);
                    while (IsBearerAuthRequestPaused() && IsRequestingNewAccessToken())
                        await HttpHelper.HttpDelayOneFrame;

                    if (!IsBearerAuthRequestPaused()) continue;

                    logger?.LogWarning("Failed retrieving new access token");
                    return new HttpSendResult(httpResponse, error);
                }

                break;
            } while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(totalTimeoutMs) || IsBearerAuthRequestPaused());

            return new HttpSendResult(httpResponse, error);
        }

        public IEnumerator SendRequest(IHttpRequest requestInput, Action<IHttpResponse, Error> callback)
        {
            var requestUniqueIdentifier = Guid.NewGuid().ToString();
            var rand = new Random();
            var retryDelayMs = initialDelayMs;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var request = requestInput;

            ApplyImplicitAuthorization(request, out var applyAuthErr);
            if (applyAuthErr.Code != ErrorCode.NONE)
            {
                callback?.Invoke(null, applyAuthErr);
                yield break;
            }

            ApplyImplicitPathParams(request);

            ApplyAdditionalData(request, additionalHeaderInfoDict, out var applyAdditionalDataErr);
            if (applyAdditionalDataErr.Code != ErrorCode.NONE)
            {
                callback?.Invoke(null, applyAdditionalDataErr);
                yield break;
            }

            if (!TryResolveUri(request))
            {
                callback?.Invoke(null, new Error(ErrorCode.INVALID_REQUEST, "Invalid uri format: ", request.Url));

                yield break;
            }

            IHttpResponse httpResponse = null;
            Error error = null;

            RequestState state;
            if (IsBearerAuthRequestPaused() && request.AuthType == HttpAuthType.BEARER)
                state = RequestState.PAUSED;
            else
                state = RequestState.RUNNING;

            var retryTimes = 0;
            do
            {
                if (state == RequestState.PAUSED)
                {
                    if (IsBearerAuthRequestPaused())
                    {
                        OnBearerAuthRejected(result => { });

                        yield return new WaitWhile(() => IsBearerAuthRequestPaused() && IsRequestingNewAccessToken());

                        if (IsBearerAuthRequestPaused())
                            logger?.LogWarning("Failed retrieving new access token, resuming");
                    }

                    state = RequestState.RESUMED;
                    stopwatch.Restart();
                    if (IsBearerAuthRequestPaused())
                    {
                        request.Headers.Remove("Authorization");
                        ApplyImplicitAuthorization(request, out var applyError);
                        if (applyError.Code != ErrorCode.NONE)
                        {
                            callback?.Invoke(null, applyAuthErr);
                            yield break;
                        }
                    }
                }

                var timeoutMs = (int)(totalTimeoutMs - stopwatch.ElapsedMilliseconds);

                uint delayMs = 0;
                if (retryTimes > 0)
                {
                    logger?.LogWarning($"Send request failed, Retry {retryTimes}/{maxRetries}");

                    request.Priority = HttpHelper.HttpRequestDefaultPriority - 1;

                    var jitterMs = rand.Next((int)(-retryDelayMs / 4), (int)(retryDelayMs / 4));
                    delayMs = (uint)(retryDelayMs + jitterMs);

                    retryDelayMs = Math.Min(retryDelayMs * 2, maxDelayMs);
                }

                HttpSendResult? sendResult = null;

                request.Id = $"{requestUniqueIdentifier}-{retryTimes}";
                Sender.AddTask(request, OnSendRequestComplete, timeoutMs, delayMs);

                yield return new WaitUntil(() => sendResult != null);

                if (sendResult != null)
                {
                    httpResponse = sendResult.Value.CallbackResponse;
                    error = sendResult.Value.CallbackError;
                }

                void OnSendRequestComplete(HttpSendResult result)
                {
                    sendResult = result;
                }

                if (httpResponse == null)
                {
                    callback?.Invoke(null, error);
                    yield break;
                }

                var requireToRetry =
                    CheckRequireRetry(request, httpResponse, error, NetworkErrorOccured, ServerErrorOccured);
                if (requireToRetry)
                {
                    retryTimes++;
                    if (retryTimes > maxRetries)
                    {
                        callback?.Invoke(httpResponse, error);
                        yield break;
                    }

                    continue;
                }

                if (httpResponse.Code == (long)HttpStatusCode.Unauthorized)
                {
                    if (state == RequestState.RESUMED || request.AuthType != HttpAuthType.BEARER)
                    {
                        state = RequestState.STOPPED;
                        callback?.Invoke(httpResponse, error);
                        UnauthorizedOccured?.Invoke(accessToken);
                        yield break;
                    }

                    state = RequestState.PAUSED;
                    OnBearerAuthRejected(result => { });

                    yield return new WaitWhile(() => IsBearerAuthRequestPaused() && IsRequestingNewAccessToken());

                    if (!IsBearerAuthRequestPaused()) continue;

                    state = RequestState.STOPPED;
                    callback?.Invoke(httpResponse, error);
                    logger?.LogWarning("Failed retrieving new access token");
                    yield break;
                }

                break;
            } while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(totalTimeoutMs) || IsBearerAuthRequestPaused());

            callback?.Invoke(httpResponse, error);
        }

        bool TryResolveUri(IHttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Url)) return false;

            if (request.Url.Contains("{") || request.Url.Contains("}")) return false;

            if (!Uri.TryCreate(baseUri, request.Url, out var uri)) return false;

            if (uri.Scheme != "https" && uri.Scheme != "http") return false;

            request.Url = uri.AbsoluteUri;

            return true;
        }

        void ApplyImplicitPathParams(IHttpRequest request)
        {
            if (request == null) return;

            if (pathParamsDict == null || pathParamsDict.Count == 0) return;

            var formattedUrl = request.Url;

            foreach (var param in pathParamsDict)
                formattedUrl = formattedUrl.Replace("{" + param.Key + "}", Uri.EscapeDataString(param.Value));

            request.Url = formattedUrl;
        }

        void ApplyImplicitAuthorization(IHttpRequest request, out Error err)
        {
            var errorResult = new Error(ErrorCode.NONE);
            err = errorResult;

            if (request == null)
            {
                errorResult = new Error(ErrorCode.INVALID_REQUEST, "request object is null");
                err = errorResult;
                return;
            }

            const string authHeaderKey = "Authorization";

            if (request.Headers != null && request.Headers.ContainsKey(authHeaderKey))
            {
                errorResult = new Error(ErrorCode.NONE);
                err = errorResult;
                return;
            }

            switch (request.AuthType)
            {
                case HttpAuthType.API_KEY:
                    if (string.IsNullOrEmpty(apiKey))
                    {
                        errorResult = new Error(ErrorCode.INVALID_REQUEST, "failed to apply auth code from url :",
                            request.Url);
                        err = errorResult;
                        break;
                    }

                    if (request.Headers != null) request.Headers[ApiKeyHeaderKey] = apiKey;
                    break;

                case HttpAuthType.BASIC:
                    // TODO: implement basic auth later
                    if (string.IsNullOrEmpty("test"))
                    {
                        errorResult = new Error(ErrorCode.INVALID_REQUEST, "failed to apply auth code from url :",
                            request.Url);
                        err = errorResult;
                        break;
                    }

                    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("test"));
                    if (request.Headers != null) request.Headers[authHeaderKey] = "Basic " + base64;
                    break;

                case HttpAuthType.BEARER:
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        errorResult = new Error(ErrorCode.IS_NOT_LOGGED_IN, "failed to apply auth code from url :",
                            request.Url);
                        err = errorResult;
                        break;
                    }

                    if (request.Headers != null) request.Headers[authHeaderKey] = "Bearer " + accessToken;
                    break;
            }
        }

        void ApplyAdditionalData(IHttpRequest request, IDictionary<string, string> additionalHeaders, out Error err)
        {
            err = new Error(ErrorCode.NONE);

            if (request == null)
            {
                err = new Error(ErrorCode.INVALID_REQUEST, "request object is null");
                return;
            }

            if (additionalHeaders != null && additionalHeaders.Count > 0)
                foreach (var headerKeyValue in additionalHeaders)
                    if (!request.Headers.ContainsKey(headerKeyValue.Key) && !string.IsNullOrEmpty(headerKeyValue.Value))
                        request.Headers.Add(headerKeyValue);
        }

        bool CheckRequireRetry(IHttpRequest httpRequest, IHttpResponse httpResponse, Error error,
            Action<IHttpRequest> onNetworkError, Action<IHttpRequest> onServerError)
        {
            var requireToRetry = false;
            if (error != null)
            {
                onNetworkError?.Invoke(httpRequest);
                requireToRetry = true;
            }
            else if (HttpParser.IsHasServerError(httpResponse))
            {
                onServerError?.Invoke(httpRequest);
                requireToRetry = true;
            }
            else
            {
                requireToRetry = HttpParser.IsInternalErrorRetriable(httpResponse);
            }

            return requireToRetry;
        }

        bool IsBearerAuthRequestPaused()
        {
            return isBanDetected;
        }

        void PauseBearerAuthRequest()
        {
            isBanDetected = true;
        }

        void ResumeBearerAuthRequest()
        {
            isBanDetected = false;
        }

        bool IsRequestingNewAccessToken()
        {
            return isRequestingNewAccessToken;
        }

        void SetRequestingNewAccessToken(bool isRequesting)
        {
            isRequestingNewAccessToken = isRequesting;
        }

        IHttpRequestSender Sender { get; set; }

        uint maxRetries;
        uint totalTimeoutMs;
        uint initialDelayMs;
        uint maxDelayMs;
        readonly IDictionary<string, string> pathParamsDict = new Dictionary<string, string>();
        readonly IDictionary<string, string> additionalHeaderInfoDict = new Dictionary<string, string>();
        Uri baseUri;

        bool isBanDetected;

        string apiKey;

        string accessToken;
        bool isRequestingNewAccessToken;
    }
}