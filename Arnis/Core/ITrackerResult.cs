using System.Collections.Generic;

namespace Arnis.Core
{
    public interface ITrackerResult
    {
        List<Solution> Results { get; set; }
        List<string> Errors { get; set; }
    }
}