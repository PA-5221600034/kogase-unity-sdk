using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Kogase.Dtos
{
    [DataContract]
    [Preserve]
    public class BeginSessionRequest
    {
        [DataMember(Name = "identifier")] public string Identifier { get; set; }
    }

    [DataContract]
    [Preserve]
    public class BeginSessionResponse
    {
        [DataMember(Name = "session_id")] public string SessionID { get; set; }
    }

    [DataContract]
    [Preserve]
    public class FinishSessionRequest
    {
        [DataMember(Name = "session_id")] public string SessionID { get; set; }
    }

    [DataContract]
    [Preserve]
    public class FinishSessionResponse
    {
        [DataMember(Name = "message")] public string Message { get; set; }
    }
}