using Arnis;

namespace Arnis.Core
{
    public interface ITracker
    {
        string Name { get;  }
        string Description { get; }
        TrackerResult Run();
    }
}