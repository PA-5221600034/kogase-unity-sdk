using System.Threading;

namespace Kogase.Core
{
    internal abstract class WaitCommand
    {
        protected System.Action onDone;
        protected CancellationToken cancellationToken;
        public abstract bool Update(float dt);

        public WaitCommand(System.Action onDone, CancellationToken cancellationToken)
        {
            this.onDone = onDone;
            this.cancellationToken = cancellationToken;
        }

        public bool IsCancelled()
        {
            return cancellationToken.IsCancellationRequested;
        }

        public void TriggerDone()
        {
            onDone?.Invoke();
        }
    }
}