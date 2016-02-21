using System.Collections.Generic;

namespace Arnis.Core
{
    public class TrackerResult : ITrackerResult
    {
        public List<Solution> Results { get; set; } = new List<Solution>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}