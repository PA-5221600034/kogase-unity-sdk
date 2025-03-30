using System;
using UnityEngine.Networking;

namespace Kogase.Core
{
    internal class WebRequestTask
    {
        public readonly int TimeoutMs;
        public IHttpRequest HttpRequest { get; }
        public Action<WebResponse> OnComplete;
        public uint DelayMs { get; }
        public int Priority => HttpRequest.Priority;
        public DateTime CreatedTimeStamp { get; }
        public WebRequestState State { get; private set; }

        public WebRequestTask(IHttpRequest httpRequest, int timeoutMs, uint delayMs)
        {
            HttpRequest = httpRequest;
            TimeoutMs = timeoutMs;
            DelayMs = delayMs;
            CreatedTimeStamp = DateTime.UtcNow;
            SetState(WebRequestState.WAITING);
        }

        public void SetComplete(WebResponse webRequestResult)
        {
            SetState(WebRequestState.COMPLETE);
            OnComplete?.Invoke(webRequestResult);
        }

        public void SetState(WebRequestState newState)
        {
            State = newState;
        }

        public UnityWebRequest CreateWebRequest()
        {
            var unityWebRequest = new UnityWebRequest(HttpRequest.Url, HttpRequest.Method);

            // Add headers
            foreach (var header in HttpRequest.Headers) unityWebRequest.SetRequestHeader(header.Key, header.Value);

            // Add body if present
            if (HttpRequest.BodyBytes != null && HttpRequest.BodyBytes.Length > 0)
                unityWebRequest.uploadHandler = new UploadHandlerRaw(HttpRequest.BodyBytes);

            // Set download handler
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            return unityWebRequest;
        }
    }
}