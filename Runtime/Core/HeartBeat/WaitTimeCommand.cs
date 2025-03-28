using System.Threading;

namespace Kogase.Core
{
    internal class WaitTimeCommand : WaitCommand
    {
        readonly double originalWaitTime;
        double waitTime;

        public WaitTimeCommand(float waitTime, System.Action onDone, CancellationToken cancellationToken) : base(onDone,
            cancellationToken)
        {
            originalWaitTime = waitTime;
            this.waitTime = waitTime;
            this.onDone = onDone;
            this.cancellationToken = cancellationToken;
        }

        public WaitTimeCommand(double waitTime, System.Action onDone, CancellationToken cancellationToken) : base(
            onDone, cancellationToken)
        {
            originalWaitTime = waitTime;
            this.waitTime = waitTime;
            this.onDone = onDone;
            this.cancellationToken = cancellationToken;
        }

        public void ResetTime()
        {
            waitTime = originalWaitTime;
        }

        public override bool Update(float dt)
        {
            waitTime -= dt;
            if (waitTime <= 0) return true;
            return false;
        }
    }
}