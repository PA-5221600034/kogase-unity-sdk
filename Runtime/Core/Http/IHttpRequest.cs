using System.Collections.Generic;

namespace Kogase.Core
{
    public interface IHttpRequest
    {
        string Id { get; set; }
        string Method { get; }
        string Url { get; set; }
        HttpAuthType AuthType { get; }
        IDictionary<string, string> Headers { get; }
        byte[] BodyBytes { get; }
        int Priority { get; set; }
    }
}