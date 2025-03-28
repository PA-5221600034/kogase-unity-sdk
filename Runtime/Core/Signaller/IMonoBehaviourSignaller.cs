namespace Kogase.Core
{
    public interface IMonoBehaviourSignaller
    {
        public System.Action<float> OnUpdateSignal { get; set; }
    }
}