using System;
using System.Collections.Generic;

namespace Kogase.Core
{
    internal class HttpRequestPrototype : IHttpRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Method { get; set; }
        public string Url { get; set; }
        public HttpAuthType AuthType { get; set; } = HttpAuthType.NONE;
        public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public byte[] BodyBytes { get; set; }
        public int Priority { get; set; } = 0;
    }
}