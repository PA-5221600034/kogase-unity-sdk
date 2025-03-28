using System.Threading;

namespace Kogase.Core
{
    internal class IndefiniteLoopCommand : WaitCommand
    {
        private System.Action onUpdate;
        private double? expectedInterval;
        private double? currentIntervalTimer;
        
        public IndefiniteLoopCommand(System.Action onUpdate, CancellationToken cancellationToken) : base(onDone: null, cancellationToken)
        {
            this.onUpdate = onUpdate;
            this.cancellationToken = cancellationToken;
        }
        
        public IndefiniteLoopCommand(double interval, System.Action onUpdate, CancellationToken cancellationToken) : base(onDone: null, cancellationToken)
        {
            this.onUpdate = onUpdate;
            this.cancellationToken = cancellationToken;

            expectedInterval = interval;
            currentIntervalTimer = interval;
        }

        public override bool Update(float dt)
        {
            if (currentIntervalTimer != null)
            {
                currentIntervalTimer -= dt;
                if (currentIntervalTimer <= 0)
                {
                    currentIntervalTimer = expectedInterval;
                }
                else
                {
                    return false;
                }
            }
            onUpdate?.Invoke();
            return false;
        }
    }
}