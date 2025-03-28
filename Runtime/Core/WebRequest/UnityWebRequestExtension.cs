using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace Kogase.Core
{
    public static class UnityWebRequestExtension
    {
        public static IHttpResponse GetHttpResponse(this UnityWebRequest request)
        {
            return new UnityWebRequestHttpResponseAdapter
            {
                Url = request.url,
                Code = request.responseCode,
                Headers = request.GetResponseHeaders(),
                BodyBytes = request.downloadHandler.data
            };
        }

        public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation reqOp)
        {
            var taskCompletionSource = new System.Threading.Tasks.TaskCompletionSource<UnityWebRequest.Result>();
            reqOp.completed += (asyncOp) => { taskCompletionSource.TrySetResult(reqOp.webRequest.result); };

            if (reqOp.isDone) taskCompletionSource.TrySetResult(reqOp.webRequest.result);

            return taskCompletionSource.Task.GetAwaiter();
        }
    }
}