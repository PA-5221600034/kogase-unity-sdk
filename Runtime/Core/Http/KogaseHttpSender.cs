using System;
using UnityEngine.Networking;

namespace Kogase.Core
{
    internal class KogaseHttpSender : IHttpSender
    {
        readonly WebRequestScheduler scheduler;

        public KogaseHttpSender(WebRequestScheduler scheduler = null)
        {
            this.scheduler = scheduler ?? new WebRequestScheduler();
        }

        public void AddTask(IHttpRequest request, Action<HttpResult> callback, int timeoutMs, uint delayTimeMs)
        {
            var task = new WebRequestTask(request, timeoutMs, delayTimeMs)
            {
                OnComplete = webResponse =>
                {
                    var result = ParseWebRequestResult(webResponse);
                    callback?.Invoke(result);
                }
            };

            scheduler.AddTask(task);
        }

        public void ClearTasks()
        {
            scheduler.ClearTasks();
        }

        public void ClearCookies(Uri baseUri)
        {
            if (baseUri != null) UnityWebRequest.ClearCookieCache(baseUri);
        }

        HttpResult ParseWebRequestResult(WebResponse webResponse)
        {
            IHttpResponse response = null;
            Error error = null;

            if (webResponse?.WebRequest == null)
            {
                error = new Error(Code.NETWORK_ERROR, "No response received");
                return new HttpResult(null, error);
            }

            var unityWebRequest = webResponse.WebRequest;

#if UNITY_2020_1_OR_NEWER
            switch (unityWebRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    response = webResponse.GetHttpResponse();
                    break;
                case UnityWebRequest.Result.ConnectionError:
                    error = new Error(Code.NETWORK_ERROR, unityWebRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    response = webResponse.GetHttpResponse();
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    error = new Error(Code.INVALID_RESPONSE, unityWebRequest.error);
                    break;
            }
#else
            if (unityWebRequest.isNetworkError)
            {
                error = new Error(Code.NETWORK_ERROR, unityWebRequest.error);
            }
            else
            {
                response = webResponse.GetHttpResponse();
            }
#endif

            return new HttpResult(response, error);
        }
    }
}