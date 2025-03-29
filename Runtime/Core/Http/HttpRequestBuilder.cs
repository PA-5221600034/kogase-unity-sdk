using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kogase.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace Kogase.Core
{
    /// <summary>
    /// Builder class for creating HTTP requests using a fluent API pattern.
    /// </summary>
    public class HttpRequestBuilder
    {
        const string GetMethod = "GET";
        const string PostMethod = "POST";
        const string PutMethod = "PUT";
        const string PatchMethod = "PATCH";
        const string DeleteMethod = "DELETE";

        readonly StringBuilder formBuilder = new(1024);
        readonly StringBuilder queryBuilder = new(256);
        readonly StringBuilder urlBuilder = new(256);
        readonly Dictionary<string, string> additionalData = new();
        HttpRequest result;

        static HttpRequestBuilder CreatePrototype(string method, string url)
        {
            var builder = new HttpRequestBuilder
            {
                result = new HttpRequest
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

        /// <summary>
        /// For endpoint URLs, we'll replace "{brackets}" key with urlEncoded (Uri-escaped) val.
        /// - Eg, "some/url/path/{namespace}/foo" will replace {namespace} key with val.
        /// - Not to be confused with WithQueryParam (GET) || WithFormParam (POST).  
        /// - WithParamParams is the singular version of WithPathParams().  
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public HttpRequestBuilder WithPathParam(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new Exception($"Path parameter key is null or empty.");

            if (string.IsNullOrEmpty(value)) throw new Exception($"The path value of key={key} is null or empty.");

            urlBuilder.Replace("{" + key + "}", Uri.EscapeDataString(value));

            return this;
        }

        /// <summary>
        /// For endpoint URLs, we'll replace "{brackets}" key with urlEncoded (Uri-escaped) val.
        /// - Eg, "some/url/path/{namespace}/foo" will replace {namespace} key with val.
        /// - Not to be confused with WithQueryParam(s) (GET) || WithFormParam (POST).  
        /// - WithParamParams() is the plural version of WithPathParam(). 
        /// </summary>
        /// <param name="pathParams"></param>
        /// <returns></returns>
        public HttpRequestBuilder WithPathParams(IDictionary<string, string> pathParams)
        {
            foreach (var param in pathParams) WithPathParam(param.Key, param.Value);

            return this;
        }

        /// <summary>
        /// For endpoint URLs, we'll replace "{brackets}" key with urlEncoded (Uri-escaped) val.
        /// - Eg, "some/url/path/{namespace}/foo" will replace {namespace} key with val.
        /// - Not to be confused with WithFormParam (POST) || WithPathParam ({bracket} val swapping) 
        /// - WithQueryParam() is the singular version of WithPathParam(). 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpRequestBuilder WithQueryParam(string key, string value)
        {
            Assert.IsNotNull(key, "query key is null");
            Assert.IsNotNull(value, $"query value is null for key {key}");

            if (string.IsNullOrEmpty(value)) return this;

            if (queryBuilder.Length > 0) queryBuilder.Append("&");

            queryBuilder.Append($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");

            return this;
        }

        /// <summary>
        /// For endpoint URLs, we'll replace "{brackets}" key with urlEncoded (Uri-escaped) val.
        /// - Eg, "some/url/path/{namespace}/foo" will replace {namespace} key with val.
        /// - Not to be confused with WithFormParam (POST) || WithPathParam ({bracket} val swapping) 
        /// - WithQueryParam() is the singular version of WithPathParam(). 
        /// </summary>
        /// <param name="queriesDict"></param>
        /// <returns></returns>
        public HttpRequestBuilder WithQueryParams(IDictionary<string, string> queriesDict)
        {
            foreach (var query in queriesDict) WithQueryParam(query.Key, query.Value);

            return this;
        }

        /// <summary>
        /// For GET-like HTTP calls only,  For POST-like HTTP calls, use WithFormParam() instead.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public HttpRequestBuilder WithQueryParam(string key, ICollection<string> values)
        {
            foreach (var value in values) WithQueryParam(Uri.EscapeDataString(key), Uri.EscapeDataString(value));

            return this;
        }

        public HttpRequestBuilder WithQueries(Dictionary<string, string> queryMap)
        {
            foreach (var queryPair in queryMap) WithQueryParam(queryPair.Key, queryPair.Value);

            return this;
        }

        public HttpRequestBuilder WithQueries<T>(T queryObject)
        {
            if (queryBuilder.Length > 0) queryBuilder.Append("&");

            queryBuilder.Append(queryObject.ToForm());

            return this;
        }

        public HttpRequestBuilder WithBasicAuth()
        {
            result.AuthType = HttpAuthType.BASIC;

            return this;
        }

        public HttpRequestBuilder WithBasicAuthWithCookie(string encodeKey)
        {
            return WithBasicAuthWithCookie(encodeKey, null);
        }

        internal HttpRequestBuilder WithBasicAuthWithCookie(string encodeKey,
            Models.IdentifierGeneratorConfig identifierGeneratorConfig)
        {
            result.AuthType = HttpAuthType.BASIC;
            var deviceProvider =
                IdentifierProvider.GetFromSystemInfo(encodeKey, identifierGeneratorConfig: identifierGeneratorConfig);
            result.Headers["cookie"] = "device-token=" + deviceProvider.Identifier;
            return this;
        }

        public HttpRequestBuilder WithBasicAuthWithCookieAndAuthTrustId(string encodeKey, string authTrustId = null)
        {
            return WithBasicAuthWithCookieAndAuthTrustId(encodeKey, null, authTrustId);
        }

        internal HttpRequestBuilder WithBasicAuthWithCookieAndAuthTrustId(string encodeKey,
            Models.IdentifierGeneratorConfig identifierGeneratorConfig, string authTrustId)
        {
            result.AuthType = HttpAuthType.BASIC;
            var deviceProvider =
                IdentifierProvider.GetFromSystemInfo(encodeKey, identifierGeneratorConfig: identifierGeneratorConfig);
            result.Headers["cookie"] = "device-token=" + deviceProvider.Identifier;
            if (!string.IsNullOrEmpty(authTrustId)) result.Headers["Auth-Trust-Id"] = authTrustId;

            return this;
        }

        public HttpRequestBuilder WithApiKeyAuth()
        {
            result.AuthType = HttpAuthType.API_KEY;

            return this;
        }

        public HttpRequestBuilder WithBasicAuth(string username, string password, bool passwordIsRequired = true)
        {
            if (string.IsNullOrEmpty(username) || (passwordIsRequired && string.IsNullOrEmpty(password)))
                throw new ArgumentException("username and password for Basic Authorization shouldn't be empty or null");

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
                throw new ArgumentException("token for Bearer Authorization shouldn't be empty or null");

            result.Headers["Authorization"] = "Bearer " + token;
            result.AuthType = HttpAuthType.BEARER;

            return this;
        }

        public HttpRequestBuilder WithContentType(MediaType mediaType)
        {
            result.Headers["Content-Type"] = mediaType.ToString();

            return this;
        }

        public HttpRequestBuilder WithContentType(string rawMediaType)
        {
            result.Headers["Content-Type"] = rawMediaType;

            return this;
        }

        public HttpRequestBuilder Accepts(MediaType mediaType)
        {
            result.Headers["Accept"] = mediaType.ToString();

            return this;
        }

        /// <summary>
        /// Add a FORM (POST-like) param key:val.
        /// - Not to be confused with WithQueryParam (GET) || WithPathParam ({bracket} val swapping)
        /// - TODO: Create plural version, WithFormParams(), like similar funcs. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpRequestBuilder WithFormParam(string key, string value)
        {
            Assert.IsNotNull(key, "form key is null");
            Assert.IsNotNull(value, $"form value is null for key {key}");

            if (string.IsNullOrEmpty(value)) return this;

            if (formBuilder.Length > 0) formBuilder.Append("&");

            formBuilder.Append($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");

            return this;
        }

        /// <summary>
        /// Add/append to additionalData form field as to be included in the request. param key:val.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpRequestBuilder AddAdditionalData(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Assert.IsNotNull(key, "Additional data key is null");
                return this;
            }

            if (string.IsNullOrEmpty(value))
            {
                Assert.IsNotNull(value, $"Additional data value is null for key {key}");
                return this;
            }

            additionalData.Add(key, value);

            return this;
        }

        public HttpRequestBuilder WithBody(string body)
        {
            if (!result.Headers.ContainsKey("Content-Type"))
                result.Headers.Add("Content-Type", MediaType.TextPlain.ToString());

            result.BodyBytes = Encoding.UTF8.GetBytes(body);

            return this;
        }

        public HttpRequestBuilder WithBody(byte[] body)
        {
            if (!result.Headers.ContainsKey("Content-Type"))
                result.Headers.Add("Content-Type", MediaType.ApplicationOctetStream.ToString());

            result.BodyBytes = body;

            return this;
        }

        public HttpRequestBuilder WithBody(FormDataContent body)
        {
            if (!result.Headers.ContainsKey("Content-Type")) result.Headers.Add("Content-Type", body.GetMediaType());

            result.BodyBytes = body.Get();

            return this;
        }

        public HttpRequestBuilder WithFormBody<T>(T body)
        {
            if (!result.Headers.ContainsKey("Content-Type"))
                result.Headers.Add("Content-Type", MediaType.ApplicationForm.ToString());

            result.BodyBytes = Encoding.UTF8.GetBytes(body.ToForm());

            return this;
        }

        public HttpRequestBuilder WithJsonBody<T>(T body)
        {
            if (!result.Headers.ContainsKey("Content-Type"))
                result.Headers.Add("Content-Type", MediaType.ApplicationJson.ToString());

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

            if (additionalData.Count > 0) WithFormParam("additionalData", additionalData.ToJsonString());

            if (formBuilder.Length > 0)
            {
                result.Headers["Content-Type"] = MediaType.ApplicationForm.ToString();
                result.BodyBytes = Encoding.UTF8.GetBytes(formBuilder.ToString());
            }

            // TODO: maybe add this later
            // if (!string.IsNullOrEmpty(gameClientVersion))
            // {
            //     this.result.Headers["App-Version"] = appVersion;
            // }
            // if (!string.IsNullOrEmpty(sdkVersion))
            // {
            //     this.result.Headers["SDK-Version"] = sdkVersion;
            // }

            result.Url = urlBuilder.ToString();

            return result;
        }

        class HttpRequest : IHttpRequest
        {
            IHttpRequest httpRequestImpl;

            public string Id { get; set; }
            public string Method { get; set; }
            public string Url { get; set; }
            public HttpAuthType AuthType { get; set; }
            public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
            public byte[] BodyBytes { get; set; }
            public int Priority { get; set; } = HttpHelper.HttpRequestDefaultPriority;
        }
    }
}