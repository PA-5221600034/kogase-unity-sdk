using System.Collections.Generic;

namespace Kogase.Core
{
    internal class WebRequestTaskOrderComparer : IComparer<WebRequestTask>
    {
        public int Compare(WebRequestTask task1, WebRequestTask task2)
        {
            if (task1 == null || task2 == null) return 0;

            if (task1.Priority < task2.Priority) return -1;

            if (task1.Priority > task2.Priority) return 1;

            if (task1.CreatedTimeStamp < task2.CreatedTimeStamp) return -1;

            if (task1.CreatedTimeStamp > task2.CreatedTimeStamp) return 1;

            return 0;
        }
    }
}