using System.Collections.Generic;

namespace TechStackTracker
{
    public interface IStackTracker
    {
        string Name { get;  }
        string Description { get; }
        List<StackReport> Run();
    }
}