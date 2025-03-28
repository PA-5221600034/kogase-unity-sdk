using System;
using System.Threading;

namespace Kogase.Core
{
    internal class WaitAFrameCommand : WaitCommand
    {
        public WaitAFrameCommand(Action onDone, CancellationToken cancellationToken) : base(onDone, cancellationToken)
        {
        }

        public override bool Update(float dt)
        {
            return true;
        }
    }
}