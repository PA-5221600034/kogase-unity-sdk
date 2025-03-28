using System.Collections.Generic;

namespace Kogase.Core
{
    public interface IHttpResponse
    {
        string Url { get; }
        long Code { get; }
        IDictionary<string, string> Headers { get; }
        byte[] BodyBytes { get; }
    }
}