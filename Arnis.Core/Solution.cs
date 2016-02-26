using System.Collections.Generic;

namespace Arnis.Core
{
    //TODO: Support project decription from sln file
    public class Solution
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }

        public List<Project> Projects { get; set; } = new List<Project>();
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();
    }
}