using System;
using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Kogase.Dtos
{
    [DataContract]
    [Preserve]
    public class HealthResponse
    {
        [DataMember(Name = "status")] public string Status { get; set; }
    }

    [DataContract]
    [Preserve]
    public class CreateProjectRequest
    {
        [DataMember(Name = "name")] public string Name { get; set; }
    }

    [DataContract]
    [Preserve]
    public class CreateProjectResponse
    {
        [DataMember(Name = "project_id")] public Guid ProjectId { get; set; }
        [DataMember(Name = "name")] public string Name { get; set; }
        [DataMember(Name = "api_key")] public string ApiKey { get; set; }
        [DataMember(Name = "owner")] public OwnerDto Owner { get; set; }
    }
    
    [DataContract]
    [Preserve]
    public class OwnerDto
    {
        [DataMember(Name = "id")] public Guid Id { get; set; }
        [DataMember(Name = "email")] public string Email { get; set; }
        [DataMember(Name = "name")] public string Name { get; set; }
    }
}