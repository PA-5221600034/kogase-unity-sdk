using System;
using UnityEngine.Networking;

namespace Kogase.Core
{
    public class KogaseWebRequest: UnityWebRequest
    {
        internal const string ResponseContentTypeHeader = "Content-Type";
        
        public string RequestId;
        public DateTime RequestTimestamp;
        public DateTime ResponseTimestamp;

        public KogaseWebRequest(Uri uri, string method) 
            : base(uri, method) { }
    }
}