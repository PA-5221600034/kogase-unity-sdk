using System;
using System.Collections.Generic;
using System.Text;
using Kogase.Utils;

namespace Kogase.Core
{
    internal class HttpRequestBuilder
    {
        const string GetMethod = "GET";
        const string PostMethod = "POST";
        const string PutMethod = "PUT";
        const string PatchMethod = "PATCH";
        const string DeleteMethod = "DELETE";

        readonly StringBuilder formBuilder = new(1024);
        readonly StringBuilder queryBuilder = new(256);
        readonly StringBuilder urlBuilder = new(256);
        HttpRequestPrototype result;

        static HttpRequestBuilder CreatePrototype(string method, string url)
        {
            var builder = new HttpRequestBuilder
            {
                result = new HttpRequestPrototype
                {
                    Method = method
                }
            };

            builder.urlBuilder.Append(url);

            return builder;
        }

        public static HttpRequestBuilder CreateGet(string url)
        {
            return CreatePrototype(GetMethod, url);
        }

        public static HttpRequestBuilder CreatePost(string url)
        {
            return CreatePrototype(PostMethod, url);
        }

        public static HttpRequestBuilder CreatePut(string url)
        {
            return CreatePrototype(PutMethod, url);
        }

        public static HttpRequestBuilder CreatePatch(string url)
        {
            return CreatePrototype(PatchMethod, url);
        }

        public static HttpRequestBuilder CreateDelete(string url)
        {
            return CreatePrototype(DeleteMethod, url);
        }

        public HttpRequestBuilder WithPathParam(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception($"Path parameter key is null or empty.");

            if (string.IsNullOrEmpty(value)) throw new Exception($"The path value of key={key} is null or empty.");

            urlBuilder.Replace("{" + key + "}", Uri.EscapeDataString(value));

            return this;
        }

        public HttpRequestBuilder WithPathParams(IDictionary<string, string> pathParams)
        {
            foreach (var param in pathParams) WithPathParam(param.Key, param.Value);

            return this;
        }

        public HttpRequestBuilder WithQueryParam(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Query parameter key is null or empty.");

            if (string.IsNullOrEmpty(value)) return this;

            if (queryBuilder.Length > 0) queryBuilder.Append("&");

            queryBuilder.Append($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");

            return this;
        }

        public HttpRequestBuilder WithQueryParams(IDictionary<string, string> queryParams)
        {
            foreach (var query in queryParams) WithQueryParam(query.Key, query.Value);

            return this;
        }

        public HttpRequestBuilder WithQueryParam(string key, ICollection<string> values)
        {
            foreach (var value in values) WithQueryParam(key, value);

            return this;
        }

        public HttpRequestBuilder WithBasicAuth()
        {
            result.AuthType = HttpAuthType.BASIC;
            return this;
        }

        public HttpRequestBuilder WithBasicAuth(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Username and password for Basic Authorization shouldn't be empty or null");

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
            result.Headers["Authorization"] = "Basic " + credentials;
            result.AuthType = HttpAuthType.BASIC;

            return this;
        }

        public HttpRequestBuilder WithBearerAuth()
        {
            result.AuthType = HttpAuthType.BEARER;
            return this;
        }

        public HttpRequestBuilder WithBearerAuth(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token for Bearer Authorization shouldn't be empty or null");

            result.Headers["Authorization"] = "Bearer " + token;
            result.AuthType = HttpAuthType.BEARER;

            return this;
        }

        public HttpRequestBuilder WithHeader(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Header key shouldn't be empty or null");

            result.Headers[key] = value;
            return this;
        }

        public HttpRequestBuilder WithContentType(HttpMediaType mediaType)
        {
            result.Headers["Content-Type"] = mediaType.ToString();
            return this;
        }

        public HttpRequestBuilder Accepts(HttpMediaType mediaType)
        {
            result.Headers["Accept"] = mediaType.ToString();
            return this;
        }

        public HttpRequestBuilder WithFormParam(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception("Form parameter key is null or empty.");

            if (string.IsNullOrEmpty(value)) return this;

            if (formBuilder.Length > 0) formBuilder.Append("&");

            formBuilder.Append($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");

            return this;
        }

        public HttpRequestBuilder WithFormParams(IDictionary<string, string> formParams)
        {
            foreach (var param in formParams) WithFormParam(param.Key, param.Value);

            return this;
        }

        public HttpRequestBuilder WithBody(string body)
        {
            if (!result.Headers.ContainsKey("Content-Type"))
                result.Headers.Add("Content-Type", HttpMediaType.TextPlain.ToString());

            result.BodyBytes = Encoding.UTF8.GetBytes(body);
            return this;
        }

        public HttpRequestBuilder WithBody(byte[] body)
        {
            if (!result.Headers.ContainsKey("Content-Type"))
                result.Headers.Add("Content-Type", HttpMediaType.ApplicationOctetStream.ToString());

            result.BodyBytes = body;
            return this;
        }

        public HttpRequestBuilder WithJsonBody<T>(T body)
        {
            if (!result.Headers.ContainsKey("Content-Type"))
                result.Headers.Add("Content-Type", HttpMediaType.ApplicationJson.ToString());

            result.BodyBytes = body.ToUtf8Json();
            return this;
        }

        public IHttpRequest GetResult()
        {
            if (queryBuilder.Length > 0)
            {
                urlBuilder.Append("?");
                urlBuilder.Append(queryBuilder);
            }

            if (formBuilder.Length > 0)
            {
                result.Headers["Content-Type"] = HttpMediaType.ApplicationForm.ToString();
                result.BodyBytes = Encoding.UTF8.GetBytes(formBuilder.ToString());
            }

            result.Url = urlBuilder.ToString();
            return result;
        }
    }
}