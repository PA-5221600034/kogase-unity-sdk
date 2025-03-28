using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace Kogase.Core
{
    public static class UnityWebRequestExtension
    {
        public static KogaseWebRequest GetUnityWebRequest(this IHttpRequest request)
        {
            var uri = new Uri(request.Url);
            var unityWebRequest = new KogaseWebRequest(uri, request.Method);

            unityWebRequest.RequestId = request.Id;
            if (request.Headers.TryGetValue("Authorization", out string value))
            {
                unityWebRequest.SetRequestHeader("Authorization", value);
            }

            foreach (var headerPair in request.Headers.Where(x => x.Key != "Authorization"))
            {
                unityWebRequest.SetRequestHeader(headerPair.Key, headerPair.Value);
            }

            if (request.BodyBytes != null)
            {
                unityWebRequest.uploadHandler = new UploadHandlerRaw(request.BodyBytes);
                unityWebRequest.disposeUploadHandlerOnDispose = true;
            }

            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            unityWebRequest.disposeDownloadHandlerOnDispose = true;

            return unityWebRequest;
        }

        public static IHttpResponse GetHttpResponse(this UnityWebRequest request)
        {
            return new UnityHttpResponseAdapter
            {
                Url = request.url, 
                Code = request.responseCode, 
                Headers = request.GetResponseHeaders(), 
                BodyBytes = request.downloadHandler.data
            };
        }

        class UnityHttpResponseAdapter : IHttpResponse
        {
            public string Url { get; set; }
            public long Code { get; set; }
            public IDictionary<string, string> Headers { get; set;  }
            public byte[] BodyBytes { get; set; }
        }

        public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation reqOp)
        {
            var taskCompletionSource = new System.Threading.Tasks.TaskCompletionSource<UnityWebRequest.Result>();
            reqOp.completed += (asyncOp) =>
            {
                taskCompletionSource.TrySetResult(reqOp.webRequest.result);
            };

            if (reqOp.isDone)
            {
                taskCompletionSource.TrySetResult(reqOp.webRequest.result);
            }    

            return taskCompletionSource.Task.GetAwaiter();
        }
    }
}