using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Kogase.Core
{
    internal class WebRequestScheduler
    {
        public Action<KogaseWebRequest, IDictionary<string ,string>, byte[], IDebugger> PreHttpRequest;
        public Action<KogaseWebRequest, IDebugger> PostHttpRequest;
        protected List<WebRequestTask> requestTask;

        WebRequestTaskOrderComparer orderComparer = new WebRequestTaskOrderComparer();

        protected IDebugger logger;
        protected HeartBeat heartBeat;
        bool stopRequested;
        
        public WebRequestScheduler()
        {
            stopRequested = false;
        }

        public void Stop()
        {
            stopRequested = true;
        }

        public void SetLogger(IDebugger newLogger)
        {
            logger = newLogger;
        }

        public void SetHeartBeat(HeartBeat hb)
        {
            Assert.IsNotNull(hb);
            heartBeat = hb;
        }

        internal async void ExecuteWebTask(WebRequestTask task)
        {
            if (stopRequested)
            {
                return;
            }
            
            if (task.DelayMs > 0)
            {
                bool isDone = false;
                heartBeat.Wait(
                    new WaitTimeCommand(
                        task.DelayMs / 1000, 
                        cancellationToken: new System.Threading.CancellationTokenSource().Token,
                        onDone: () =>
                        {
                            isDone = true;
                        }
                    )
                );
                while (!isDone)
                {
                    await System.Threading.Tasks.Task.Yield();
                }
            }

            using (KogaseWebRequest webRequest = task.CreateWebRequest())
            {
                using (var webRequestCancelTokenSource = new System.Threading.CancellationTokenSource())
                {
                    double timeoutSeconds = task.TimeoutMs / 1000;
                    bool isTimeout = false;
                    heartBeat.Wait(new WaitTimeCommand(waitTime: timeoutSeconds, cancellationToken: webRequestCancelTokenSource.Token, onDone: () =>
                    {
                        logger?.LogWarning($"{webRequest.method} {webRequest.uri} reached timeout");
                        webRequest?.Abort();
                        isTimeout = true;
                    }));
                    
                    webRequest.RequestTimestamp = DateTime.UtcNow;
                    PreHttpRequest?.Invoke(webRequest, task.HttpRequest.Headers, task.HttpRequest.BodyBytes, logger);
                    
                    var asyncOp = webRequest.SendWebRequest();
                    while (!asyncOp.isDone && !isTimeout)
                    {
                        await System.Threading.Tasks.Task.Yield();
                    }
                    webRequest.ResponseTimestamp = DateTime.UtcNow;
                    PostHttpRequest?.Invoke(webRequest, logger);
                    
                    webRequestCancelTokenSource.Cancel();
                    task.SetComplete(webRequest);
                }
            }
        }
    }
}