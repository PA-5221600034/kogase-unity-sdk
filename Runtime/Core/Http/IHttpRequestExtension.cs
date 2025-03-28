using System;
using System.Linq;
using UnityEngine.Networking;

namespace Kogase.Core
{
    // ReSharper disable once InconsistentNaming
    public static class IHttpRequestExtension
    {
        public static KogaseWebRequest GetUnityWebRequest(this IHttpRequest request)
        {
            var uri = new Uri(request.Url);
            var unityWebRequest = new KogaseWebRequest(uri, request.Method);

            unityWebRequest.RequestId = request.Id;
            if (request.Headers.TryGetValue("Authorization", out var value))
                unityWebRequest.SetRequestHeader("Authorization", value);

            foreach (var headerPair in request.Headers.Where(x => x.Key != "Authorization"))
                unityWebRequest.SetRequestHeader(headerPair.Key, headerPair.Value);

            if (request.BodyBytes != null)
            {
                unityWebRequest.uploadHandler = new UploadHandlerRaw(request.BodyBytes);
                unityWebRequest.disposeUploadHandlerOnDispose = true;
            }

            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            unityWebRequest.disposeDownloadHandlerOnDispose = true;

            return unityWebRequest;
        }
    }
}