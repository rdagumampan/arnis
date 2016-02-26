using System.Collections.Generic;

namespace Arnis.Core
{
    //TODO: Support project decription from AssemblyInfo.cs
    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }

        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();
    }
}