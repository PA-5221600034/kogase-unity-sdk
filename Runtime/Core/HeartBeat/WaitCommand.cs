using System.Threading;

namespace Kogase.Core
{
    internal abstract class WaitCommand
    {
        protected System.Action OnDone;
        protected CancellationToken CancellationToken;
        public abstract bool Update(float dt);

        public WaitCommand(System.Action onDone, CancellationToken cancellationToken)
        {
            this.OnDone = onDone;
            this.CancellationToken = cancellationToken;
        }

        public bool IsCancelled()
        {
            return CancellationToken.IsCancellationRequested;
        }

        public void TriggerDone()
        {
            OnDone?.Invoke();
        }
    }
}