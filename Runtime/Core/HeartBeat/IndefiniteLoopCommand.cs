using System.Threading;

namespace Kogase.Core
{
    internal class IndefiniteLoopCommand : WaitCommand
    {
        readonly System.Action onUpdate;
        readonly double? expectedInterval;
        double? currentIntervalTimer;

        public IndefiniteLoopCommand(System.Action onUpdate, CancellationToken cancellationToken) : base(null,
            cancellationToken)
        {
            this.onUpdate = onUpdate;
            CancellationToken = cancellationToken;
        }

        public IndefiniteLoopCommand(double interval, System.Action onUpdate, CancellationToken cancellationToken) :
            base(null, cancellationToken)
        {
            this.onUpdate = onUpdate;
            CancellationToken = cancellationToken;

            expectedInterval = interval;
            currentIntervalTimer = interval;
        }

        public override bool Update(float dt)
        {
            if (currentIntervalTimer != null)
            {
                currentIntervalTimer -= dt;
                if (currentIntervalTimer <= 0)
                    currentIntervalTimer = expectedInterval;
                else
                    return false;
            }

            onUpdate?.Invoke();
            return false;
        }
    }
}