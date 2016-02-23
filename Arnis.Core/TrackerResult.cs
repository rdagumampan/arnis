using System.Collections.Generic;

namespace Arnis.Core
{
    public class TrackerResult : ITrackerResult
    {
        public List<Solution> Solutions { get; set; } = new List<Solution>();
        public List<string> Logs { get; set; } = new List<string>();
    }
}