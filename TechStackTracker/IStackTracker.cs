using System.Collections.Generic;

namespace Winterfern
{
    public interface IStackTracker
    {
        string Name { get;  }
        string Description { get; }
        StackReport Run();
    }
}