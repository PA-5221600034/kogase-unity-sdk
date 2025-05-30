using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Kogase.Core
{
    internal class WebRequestScheduler
    {
        protected readonly List<WebRequestTask> RequestTasks = new();
        protected readonly WebRequestTaskOrderComparer OrderComparer = new();
        readonly CoroutineRunner coroutineRunner;
        bool stopRequested;

        public WebRequestScheduler(CoroutineRunner coroutineRunner = null)
        {
            this.coroutineRunner = coroutineRunner ?? new CoroutineRunner();
            stopRequested = false;
        }

        public void Stop()
        {
            stopRequested = true;
        }

        internal async void ExecuteWebTask(WebRequestTask task)
        {
            if (stopRequested) return;

            // Handle delay if needed
            if (task.DelayMs > 0) await Task.Delay((int)task.DelayMs);

            task.SetState(WebRequestState.ON_PROCESS);

            using (var webRequest = task.CreateWebRequest())
            {
                using (var cancelTokenSource = new CancellationTokenSource())
                {
                    // Set up timeout
                    var isTimeout = false;
                    var timeoutTask = Task.Delay(task.TimeoutMs, cancelTokenSource.Token).ContinueWith(_ =>
                    {
                        Debug.LogWarning($"{webRequest.method} {webRequest.url} reached timeout");
                        webRequest.Abort();
                        isTimeout = true;
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);

                    // Track timestamps
                    var sentTimestamp = DateTime.UtcNow;

                    // Send the request
                    var asyncOp = webRequest.SendWebRequest();

                    // Wait for completion or timeout
                    while (!asyncOp.isDone && !isTimeout) await Task.Yield();

                    // Record response timestamp
                    var responseTimestamp = DateTime.UtcNow;

                    // Cancel the timeout timer
                    cancelTokenSource.Cancel();

                    // Create response and complete the task
                    var webResponse = new WebResponse(webRequest, sentTimestamp, responseTimestamp);
                    task.SetComplete(webResponse);
                }
            }
        }

        public void AddTask(WebRequestTask task)
        {
            RequestTasks.Add(task);
            ExecuteWebTask(task);
        }

        public void ClearTasks()
        {
            Stop();
            RequestTasks.Clear();
        }
    }
}