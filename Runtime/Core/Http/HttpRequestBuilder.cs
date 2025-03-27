using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
namespace Kogase.Core.Http
{
    /// <summary>
    /// Builder class for creating HTTP requests using a fluent API pattern.
    /// </summary>
    public class HttpRequestBuilder
    {
        private HttpMethod method;
        private string baseUrl;
        private string path;
        private Dictionary<string, string> queryParams;
        private Dictionary<string, string> pathParams;
        private Dictionary<string, string> headers;
        private string body;
        private byte[] bodyBytes;
        private List<IMultipartFormSection> formData;
        private bool useFormData;
        private FileStream fileStream;

        /// <summary>
        /// Creates a new HttpRequestBuilder with GET method
        /// </summary>
        public static HttpRequestBuilder CreateGet(string url)
        {
            return new HttpRequestBuilder(HttpMethod.Get, url);
        }

        /// <summary>
        /// Creates a new HttpRequestBuilder with POST method
        /// </summary>
        public static HttpRequestBuilder CreatePost(string url)
        {
            return new HttpRequestBuilder(HttpMethod.Post, url);
        }

        /// <summary>
        /// Creates a new HttpRequestBuilder with PUT method
        /// </summary>
        public static HttpRequestBuilder CreatePut(string url)
        {
            return new HttpRequestBuilder(HttpMethod.Put, url);
        }

        /// <summary>
        /// Creates a new HttpRequestBuilder with DELETE method
        /// </summary>
        public static HttpRequestBuilder CreateDelete(string url)
        {
            return new HttpRequestBuilder(HttpMethod.Delete, url);
        }

        /// <summary>
        /// Creates a new HttpRequestBuilder with PATCH method
        /// </summary>
        public static HttpRequestBuilder CreatePatch(string url)
        {
            return new HttpRequestBuilder(HttpMethod.Patch, url);
        }

        private HttpRequestBuilder(HttpMethod method, string url)
        {
            this.method = method;
            
            // Parse URL to separate base URL and path
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                baseUrl = $"{uri.Scheme}://{uri.Authority}";
                path = uri.PathAndQuery;
            }
            else
            {
                baseUrl = "";
                path = url;
            }

            queryParams = new Dictionary<string, string>();
            pathParams = new Dictionary<string, string>();
            headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Sets a path parameter for URL substitution
        /// </summary>
        public HttpRequestBuilder WithPathParam(string name, string value)
        {
            if (string.IsNullOrEmpty(name) || value == null)
            {
                return this;
            }

            pathParams[name] = value;
            return this;
        }

        /// <summary>
        /// Sets multiple path parameters for URL substitution
        /// </summary>
        public HttpRequestBuilder WithPathParams(Dictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return this;
            }

            foreach (var param in parameters)
            {
                WithPathParam(param.Key, param.Value);
            }

            return this;
        }

        /// <summary>
        /// Sets a query parameter
        /// </summary>
        public HttpRequestBuilder WithQueryParam(string name, string value)
        {
            if (string.IsNullOrEmpty(name) || value == null)
            {
                return this;
            }

            queryParams[name] = value;
            return this;
        }

        /// <summary>
        /// Sets multiple query parameters
        /// </summary>
        public HttpRequestBuilder WithQueryParams(Dictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return this;
            }

            foreach (var param in parameters)
            {
                WithQueryParam(param.Key, param.Value);
            }

            return this;
        }

        /// <summary>
        /// Sets a collection of values for the same query parameter
        /// </summary>
        public HttpRequestBuilder WithQueryParam(string name, IEnumerable<string> values)
        {
            if (string.IsNullOrEmpty(name) || values == null)
            {
                return this;
            }

            foreach (string value in values)
            {
                WithQueryParam(name, value);
            }

            return this;
        }

        /// <summary>
        /// Sets a header
        /// </summary>
        public HttpRequestBuilder WithHeader(string name, string value)
        {
            if (string.IsNullOrEmpty(name) || value == null)
            {
                return this;
            }

            headers[name] = value;
            return this;
        }

        /// <summary>
        /// Sets multiple headers
        /// </summary>
        public HttpRequestBuilder WithHeaders(Dictionary<string, string> headers)
        {
            if (headers == null)
            {
                return this;
            }

            foreach (var header in headers)
            {
                WithHeader(header.Key, header.Value);
            }

            return this;
        }

        /// <summary>
        /// Sets API Key authentication header
        /// </summary>
        public HttpRequestBuilder WithApiKeyAuth(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogWarning("API Key is null or empty");
                return this;
            }

            return WithHeader("X-API-Key", apiKey);
        }

        /// <summary>
        /// Sets the request body as a JSON object
        /// </summary>
        public HttpRequestBuilder WithBody(object bodyObj)
        {
            if (bodyObj == null)
            {
                body = null;
                return this;
            }

            try
            {
                body = JsonUtility.ToJson(bodyObj);
                
                // Set content type header if not already set
                if (!headers.ContainsKey("Content-Type"))
                {
                    headers["Content-Type"] = "application/json";
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error serializing request body: {e.Message}");
                body = null;
            }
            
            return this;
        }

        /// <summary>
        /// Sets the request body as a raw string
        /// </summary>
        public HttpRequestBuilder WithStringBody(string content, string contentType = "text/plain")
        {
            body = content;
            
            if (!headers.ContainsKey("Content-Type"))
            {
                headers["Content-Type"] = contentType;
            }
            
            return this;
        }

        /// <summary>
        /// Sets the request body as raw bytes
        /// </summary>
        public HttpRequestBuilder WithByteBody(byte[] bytes, string contentType = "application/octet-stream")
        {
            bodyBytes = bytes;
            
            if (!headers.ContainsKey("Content-Type"))
            {
                headers["Content-Type"] = contentType;
            }
            
            return this;
        }

        /// <summary>
        /// Sets form data to be sent with the request
        /// </summary>
        public HttpRequestBuilder WithFormData(Dictionary<string, string> formParameters)
        {
            if (formParameters == null || formParameters.Count == 0)
            {
                return this;
            }

            useFormData = true;
            formData = new List<IMultipartFormSection>();
            
            foreach (var param in formParameters)
            {
                formData.Add(new MultipartFormDataSection(param.Key, param.Value));
            }
            
            return this;
        }

        /// <summary>
        /// Adds a file to be uploaded with the request
        /// </summary>
        public HttpRequestBuilder WithFile(string fieldName, byte[] fileData, string fileName, string contentType = "application/octet-stream")
        {
            if (fileData == null || string.IsNullOrEmpty(fieldName))
            {
                return this;
            }
            
            useFormData = true;
            
            if (formData == null)
            {
                formData = new List<IMultipartFormSection>();
            }
            
            formData.Add(new MultipartFormFileSection(fieldName, fileData, fileName, contentType));
            
            return this;
        }

        /// <summary>
        /// Sets the Content-Type header
        /// </summary>
        public HttpRequestBuilder WithContentType(string contentType)
        {
            return WithHeader("Content-Type", contentType);
        }

        /// <summary>
        /// Builds the HTTP request
        /// </summary>
        public IHttpRequest Build()
        {
            // Process URL with path params
            string processedPath = ProcessPathParams(path, pathParams);
            string url = baseUrl + processedPath;
            
            // Append query params
            if (queryParams != null && queryParams.Count > 0)
            {
                url = AppendQueryParams(url, queryParams);
            }
            
            // Create the HttpRequest
            HttpRequest request = new HttpRequest
            {
                Method = method,
                Url = url,
                Headers = new Dictionary<string, string>(headers)
            };
            
            if (useFormData && formData != null && formData.Count > 0)
            {
                request.FormData = formData;
            }
            else if (bodyBytes != null)
            {
                request.BodyBytes = bodyBytes;
            }
            else
            {
                request.Body = body;
            }
            
            return request;
        }

        private string ProcessPathParams(string inputPath, Dictionary<string, string> pathParams)
        {
            if (pathParams == null || pathParams.Count == 0)
            {
                return inputPath;
            }
            
            string processedPath = inputPath;
            
            foreach (KeyValuePair<string, string> pathParam in pathParams)
            {
                // Replace {paramName} with value
                processedPath = processedPath.Replace("{" + pathParam.Key + "}", 
                    Uri.EscapeDataString(pathParam.Value));
            }
            
            return processedPath;
        }

        private string AppendQueryParams(string url, Dictionary<string, string> queryParams)
        {
            if (queryParams == null || queryParams.Count == 0)
            {
                return url;
            }
            
            StringBuilder urlBuilder = new StringBuilder(url);
            bool firstParam = !url.Contains("?");
            
            foreach (var param in queryParams)
            {
                urlBuilder.Append(firstParam ? "?" : "&");
                urlBuilder.Append(Uri.EscapeDataString(param.Key));
                urlBuilder.Append("=");
                urlBuilder.Append(Uri.EscapeDataString(param.Value));
                firstParam = false;
            }
            
            return urlBuilder.ToString();
        }
    }

    /// <summary>
    /// HTTP Methods supported by the HttpRequestBuilder
    /// </summary>
    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete,
        Patch
    }
} 