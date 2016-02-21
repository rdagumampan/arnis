using System.Collections.Generic;
using Arnis;

namespace Arnis.Core
{
    public interface ITracker
    {
        string Name { get;  }
        string Description { get; }

        TrackerResult Run(string workspace, List<string> skipList);
    }
}