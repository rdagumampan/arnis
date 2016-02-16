using System.Collections.Generic;

namespace Arnis
{
    public interface IStackTracker
    {
        string Name { get;  }
        string Description { get; }
        StackReport Run();
    }
}