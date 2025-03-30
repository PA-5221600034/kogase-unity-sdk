using System.Collections.Generic;

namespace Kogase.Core
{
    internal class HttpResponsePrototype : IHttpResponse
    {
        public string Url { get; set; }
        public long Code { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public byte[] BodyBytes { get; set; }
    }
}