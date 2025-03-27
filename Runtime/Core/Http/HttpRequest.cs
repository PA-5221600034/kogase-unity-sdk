using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
namespace Kogase.Core.Http
{
    /// <summary>
    /// Implementation of IHttpRequest representing an HTTP request
    /// </summary>
    public class HttpRequest : IHttpRequest
    {
        /// <summary>
        /// The HTTP method for the request
        /// </summary>
        public HttpMethod Method { get; set; }
        
        /// <summary>
        /// The complete URL for the request
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Headers to include with the request
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }
        
        /// <summary>
        /// String content body of the request (e.g., JSON)
        /// </summary>
        public string Body { get; set; }
        
        /// <summary>
        /// Binary content body of the request
        /// </summary>
        public byte[] BodyBytes { get; set; }
        
        /// <summary>
        /// Form data for the request
        /// </summary>
        public List<IMultipartFormSection> FormData { get; set; }
        
        /// <summary>
        /// Creates a new HttpRequest
        /// </summary>
        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates a UnityWebRequest from this request
        /// </summary>
        public UnityWebRequest CreateWebRequest()
        {
            UnityWebRequest webRequest;
            
            if (FormData != null && FormData.Count > 0)
            {
                // Multipart form data
                webRequest = UnityWebRequest.Post(Url, FormData);
            }
            else if (!string.IsNullOrEmpty(Body))
            {
                // String content (JSON, text, etc.)
                webRequest = new UnityWebRequest(Url, Method.ToString());
                byte[] bodyRaw = Encoding.UTF8.GetBytes(Body);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
            }
            else if (BodyBytes != null)
            {
                // Binary content
                webRequest = new UnityWebRequest(Url, Method.ToString());
                webRequest.uploadHandler = new UploadHandlerRaw(BodyBytes);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
            }
            else
            {
                // No body
                switch (Method)
                {
                    case HttpMethod.Get:
                        webRequest = UnityWebRequest.Get(Url);
                        break;
                    case HttpMethod.Delete:
                        webRequest = UnityWebRequest.Delete(Url);
                        break;
                    case HttpMethod.Put:
                        webRequest = UnityWebRequest.Put(Url, new byte[0]);
                        break;
                    case HttpMethod.Post:
                        webRequest = UnityWebRequest.Post(Url, "");
                        break;
                    case HttpMethod.Patch:
                        webRequest = new UnityWebRequest(Url, "PATCH");
                        webRequest.downloadHandler = new DownloadHandlerBuffer();
                        break;
                    default:
                        webRequest = UnityWebRequest.Get(Url);
                        break;
                }
            }
            
            // Add headers
            foreach (var header in Headers)
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }
            
            return webRequest;
        }
    }
} 