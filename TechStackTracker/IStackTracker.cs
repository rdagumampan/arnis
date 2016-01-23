using System.Collections.Generic;

namespace TechStackTracker
{
    public interface IStackTracker
    {
        string Name { get;  }
        string Description { get; }
        StackReport Run();
    }
}