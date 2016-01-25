using System.Collections.Generic;

namespace Winterfern
{
    public class Solution
    {
        public string Name { get; set; }
        public string Location { get; set; }

        public List<Project> Projects { get; set; } = new List<Project>();
        public List<SolutionDependency> Dependencies { get; set; } = new List<SolutionDependency>();
    }
}