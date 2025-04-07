using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Kogase.Dtos
{
    [DataContract]
    [Preserve]
    public class RecordEventRequest
    {
        [DataMember(Name = "identifier")] public string Identifier { get; set; }
        [DataMember(Name = "event_type")] public string EventType { get; set; }
        [DataMember(Name = "event_name")] public string EventName { get; set; }
        [DataMember(Name = "payloads")] public Dictionary<string, object> Payloads { get; set; } = new();
        [DataMember(Name = "timestamp")] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    [DataContract]
    [Preserve]
    public class RecordEventResponse
    {
        [DataMember(Name = "message")] public string Message { get; set; }
    }

    [DataContract]
    [Preserve]
    public class RecordEventsRequest
    {
        [DataMember(Name = "events")] public List<RecordEventRequest> Events { get; set; } = new();
    }

    [DataContract]
    [Preserve]
    public class RecordEventsResponse
    {
        [DataMember(Name = "message")] public string Message { get; set; }
        [DataMember(Name = "count")] public int Count { get; set; }
    }
}