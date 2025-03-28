using System.Collections.Generic;

namespace Kogase.Core
{
    internal class HeartBeat
    {
        bool acceptCommand;
        readonly List<WaitCommand> newCommands;
        readonly List<WaitCommand> waitCommands;
        readonly List<WaitCommand> removedCommands;

        public HeartBeat()
        {
            newCommands = new List<WaitCommand>();
            waitCommands = new List<WaitCommand>();
            removedCommands = new List<WaitCommand>();
            acceptCommand = true;
            KogaseSDKMain.AddOnUpdateListener(OnUpdate);
        }

        ~HeartBeat()
        {
            Reset();
        }

        public void Reset()
        {
            acceptCommand = false;
            KogaseSDKMain.RemoveOnUpdateListener(OnUpdate);
            lock (waitCommands)
            {
                waitCommands.Clear();
            }
        }

        public void Wait(WaitCommand newCommand)
        {
            if (acceptCommand)
                lock (newCommands)
                {
                    newCommands.Add(newCommand);
                }
        }

        void OnUpdate(float dt)
        {
            removedCommands.Clear();
            lock (waitCommands)
            {
                lock (newCommands)
                {
                    if (newCommands.Count > 0)
                    {
                        waitCommands.AddRange(newCommands);
                        newCommands.Clear();
                    }
                }

                for (var i = 0; i < waitCommands.Count; i++)
                {
                    if (waitCommands[i].IsCancelled())
                    {
                        removedCommands.Add(waitCommands[i]);
                        continue;
                    }

                    if (waitCommands[i].Update(dt)) removedCommands.Add(waitCommands[i]);
                }

                foreach (var removedCommand in removedCommands)
                {
                    waitCommands.Remove(removedCommand);
                    if (!removedCommand.IsCancelled()) removedCommand?.TriggerDone();
                }
            }
        }
    }
}