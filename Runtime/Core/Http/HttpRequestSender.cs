using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using Kogase.Models;

namespace Kogase.Core
{
    internal class HttpRequestSender : IHttpRequestSender
    {
        readonly WebRequestScheduler httpTaskScheduler;

        HeartBeat heartBeat;

        static HashSet<string> _clearedCookiesUrl;

        public HttpRequestSender(WebRequestScheduler httpTaskScheduler)
        {
            this.httpTaskScheduler = httpTaskScheduler;
            if (_clearedCookiesUrl == null) _clearedCookiesUrl = new HashSet<string>();
        }

        internal void SetHeartBeat(HeartBeat hb)
        {
            heartBeat = hb;
            httpTaskScheduler?.SetHeartBeat(hb);
        }

        public void AddTask(IHttpRequest request, Action<HttpSendResult> callback, int timeoutMs, uint delayTimeMs = 0)
        {
            var newScheduler = new WebRequestTask(request, timeoutMs, delayTimeMs)
            {
                OnComplete = (sentWebRequest) =>
                {
                    var responseResult = ParseWebRequestResult(sentWebRequest);
                    callback?.Invoke(responseResult);
                }
            };
            heartBeat.Wait(new WaitAFrameCommand(
                cancellationToken: new System.Threading.CancellationTokenSource().Token, onDone:
                () => { httpTaskScheduler.ExecuteWebTask(newScheduler); }));
        }

        public void ClearTasks()
        {
            httpTaskScheduler.Stop();
        }

        public void ClearCookies(Uri uri)
        {
            ClearCookiesThreadSafe(uri);
        }

        internal void ClearCookiesThreadSafe(Uri uri)
        {
            var retval = new KogasePromise<Error>();
            var uriString = uri.ToString();

            if (_clearedCookiesUrl.Contains(uriString) || string.IsNullOrEmpty(uriString))
                retval.Resolve();
            else
                try
                {
                    if (heartBeat != null)
                    {
                        heartBeat.Wait(new WaitAFrameCommand(
                            cancellationToken: new System.Threading.CancellationTokenSource().Token, onDone: () =>
                            {
                                if (_clearedCookiesUrl.Add(uriString)) UnityWebRequest.ClearCookieCache(uri);

                                retval.Resolve();
                            }));
                    }
                    else
                    {
                        UnityWebRequest.ClearCookieCache(uri);
                        retval.Resolve();
                    }
                }
                catch (Exception ex)
                {
                    retval.Reject(new Error(ErrorCode.ERROR_FROM_EXCEPTION, ex.Message));
                }
        }

        HttpSendResult ParseWebRequestResult(KogaseWebRequest webRequest)
        {
            IHttpResponse callBackResponse = null;
            Error callBackError = null;
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    callBackResponse = webRequest.GetHttpResponse();
                    break;
                case UnityWebRequest.Result.ConnectionError:
                    callBackError = new Error(ErrorCode.NETWORK_ERROR);
                    break;
            }

            return new HttpSendResult(callBackResponse, callBackError);
        }
    }
}