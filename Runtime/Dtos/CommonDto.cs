using System;

namespace Kogase.Dtos
{
    [Serializable]
    public class HealthResponse
    {
        public string Status { get; set; }
    }
    
    [Serializable]
    public class CreateProjectRequest
    {
        public string Name { get; set; }
    }

    [Serializable]
    public class CreateProjectResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public Guid OwnerId { get; set; }
    }
}