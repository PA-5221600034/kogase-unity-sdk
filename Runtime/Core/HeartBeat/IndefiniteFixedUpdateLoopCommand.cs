using System.Threading;

namespace Kogase.Core
{
    internal class IndefiniteFixedUpdateLoopCommand : WaitCommand
    {
        readonly System.Action<float> onUpdate;

        public IndefiniteFixedUpdateLoopCommand(System.Action<float> onUpdate, CancellationToken cancellationToken) :
            base(null, cancellationToken)
        {
            this.onUpdate = onUpdate;
            this.CancellationToken = cancellationToken;
        }

        public override bool Update(float dt)
        {
            onUpdate?.Invoke(dt);
            return false;
        }
    }
}