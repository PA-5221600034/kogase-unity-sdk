using System.Threading;

namespace Kogase.Core
{
    internal class IndefiniteFixedUpdateLoopCommand : WaitCommand
    {
        private System.Action<float> onUpdate;
        
        public IndefiniteFixedUpdateLoopCommand(System.Action<float> onUpdate, CancellationToken cancellationToken) : base(onDone: null, cancellationToken)
        {
            this.onUpdate = onUpdate;
            this.cancellationToken = cancellationToken;
        }
        
        public override bool Update(float dt)
        {
            onUpdate?.Invoke(dt);
            return false;
        }
    }
}