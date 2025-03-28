using System;
namespace Kogase.Models
{
    [Serializable]
    public class PublicIp
    {
        public string Ip;
    }
    
    public abstract class IdentifierGeneratorConfig
    {
        public bool RandomizeIdentifier { get; } = false;

        public IdentifierGeneratorConfig(bool randomizeIdentifier)
        {
            RandomizeIdentifier = randomizeIdentifier;
        }
    }
}
