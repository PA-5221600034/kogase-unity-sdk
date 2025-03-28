using System;
using System.Collections.Generic;

namespace Kogase.Core
{
    internal class WebRequestTask
    {
        public readonly int TimeoutMs;
        public IHttpRequest HttpRequest
        {
            get;
            private set;
        }

        public System.Action<KogaseWebRequest> OnComplete;

        public uint DelayMs
        {
            get;
            private set;
        }

        public int Priority
        {
            get
            {
                return HttpRequest.Priority;
            }
        }

        public DateTime CreatedTimestamp
        {
            get;
            private set;
        }

        public WebRequestState State
        {
            get;
            private set;
        }

        public WebRequestTask(IHttpRequest httpRequest, int timeoutMs, uint delayMs)
        {
            HttpRequest = httpRequest;
            DelayMs = delayMs;
            this.TimeoutMs = timeoutMs;
            CreatedTimestamp = DateTime.UtcNow;
            SetState(WebRequestState.WAITING);
        }

        public void SetComplete(KogaseWebRequest webRequestResult)
        {
            SetState(WebRequestState.COMPLETE);
            OnComplete?.Invoke(webRequestResult);
        }

        public void SetState(WebRequestState newState)
        {
            State = newState;
        }

        public KogaseWebRequest CreateWebRequest()
        {
            KogaseWebRequest unityWebRequest = HttpRequest.GetUnityWebRequest();
            return unityWebRequest;
        }
    }

    internal class WebRequestTaskOrderComparer : IComparer<WebRequestTask>
    {
        public int Compare(WebRequestTask task1, WebRequestTask task2)
        {
            if (task1.Priority < task2.Priority)
            {
                return -1;
            }
            else if (task1.Priority > task2.Priority)
            {
                return 1;
            }

            if (task1.CreatedTimestamp < task2.CreatedTimestamp)
            {
                return -1;
            }
            else if (task1.CreatedTimestamp > task2.CreatedTimestamp)
            {
                return 1;
            }

            return 0;
        }
    }

    internal enum WebRequestState
    {
        WAITING,
        ON_PROCESS,
        COMPLETE
    }
}