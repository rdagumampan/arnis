using Arnis;

namespace Arnis.Core
{
    public interface IStackTracker
    {
        string Name { get;  }
        string Description { get; }
        StackReport Run();
    }
}