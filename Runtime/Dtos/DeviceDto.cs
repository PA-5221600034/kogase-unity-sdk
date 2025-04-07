using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Kogase.Dtos
{
    [DataContract]
    [Preserve]
    public class CreateOrUpdateDeviceRequest
    {
        [DataMember(Name = "identifier")] public string Identifier { get; set; }
        [DataMember(Name = "platform")] public string Platform { get; set; }

        [DataMember(Name = "platform_version")]
        public string PlatformVersion { get; set; }

        [DataMember(Name = "app_version")] public string AppVersion { get; set; }
    }

    [DataContract]
    [Preserve]
    public class CreateOrUpdateDeviceResponse
    {
        [DataMember(Name = "device_id")] public string DeviceID { get; set; }
        [DataMember(Name = "identifier")] public string Identifier { get; set; }
        [DataMember(Name = "platform")] public string Platform { get; set; }

        [DataMember(Name = "platform_version")]
        public string PlatformVersion { get; set; }

        [DataMember(Name = "app_version")] public string AppVersion { get; set; }
        [DataMember(Name = "first_seen")] public string FirstSeen { get; set; }
        [DataMember(Name = "last_seen")] public string LastSeen { get; set; }
        [DataMember(Name = "ip_address")] public string IpAddress { get; set; }
        [DataMember(Name = "country")] public string Country { get; set; }
    }
}