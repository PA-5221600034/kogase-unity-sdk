using System.Diagnostics;
namespace Kogase.Core
{
    public class TimeManager
    {
        Stopwatch timelapseSinceSessionStart;
        public Stopwatch TimelapseSinceSessionStart => timelapseSinceSessionStart;
        
        public TimeManager()
        {
            // StartGameSession();
        }
        
        ~TimeManager()
        {
            // StopGameSession();
        }
    }
}