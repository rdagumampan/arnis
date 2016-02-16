using System.Collections.Generic;

namespace Arnis.Core
{
    public class Solution
    {
        public string Name { get; set; }
        public string Location { get; set; }

        public List<Project> Projects { get; set; } = new List<Project>();
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();
    }
}