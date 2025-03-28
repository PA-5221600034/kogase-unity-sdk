using System.Collections.Generic;

namespace Kogase.Core
{
    internal class HttpHeaderHelper
    {
        public static string GetHeaderValue(IDictionary<string, string> header, string key)
        {
            if (header == null) return null;
            if (header.TryGetValue(key, out var retval)) return retval;
            return header.TryGetValue(key.ToLower(), out retval) ? retval : null;
        }
    }
}