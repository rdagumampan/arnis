using System.Collections.Generic;

namespace Arnis.Core
{
    public interface ITrackerResult
    {
        List<Solution> Solutions { get; set; }
        List<string> Logs { get; set; }
    }
}