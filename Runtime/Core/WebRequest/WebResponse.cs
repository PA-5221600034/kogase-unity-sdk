using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Kogase.Core
{
    internal class WebResponse
    {
        public readonly UnityWebRequest WebRequest;
        public readonly DateTime SentTimestamp;
        public readonly DateTime ResponseTimestamp;

        public WebResponse(UnityWebRequest webRequest, DateTime sentTimestamp, DateTime responseTimestamp)
        {
            WebRequest = webRequest;
            SentTimestamp = sentTimestamp;
            ResponseTimestamp = responseTimestamp;
        }

        public IHttpResponse GetHttpResponse()
        {
            return new HttpResponsePrototype
            {
                Url = WebRequest.url,
                Code = WebRequest.responseCode,
                Headers = GetResponseHeaders(WebRequest),
                BodyBytes = WebRequest.downloadHandler.data
            };
        }

        Dictionary<string, string> GetResponseHeaders(UnityWebRequest request)
        {
            var result = new Dictionary<string, string>();

            if (request.GetResponseHeaders() != null)
                foreach (var header in request.GetResponseHeaders())
                    result[header.Key] = header.Value;

            return result;
        }
    }
}