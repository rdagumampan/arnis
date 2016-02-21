using System.Collections.Generic;

namespace Arnis.Core
{
    public class Workspace
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Owners { get; set; }
        public List<Solution> Solutions { get; set; }
    }
}