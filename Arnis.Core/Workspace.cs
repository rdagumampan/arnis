using System.Collections.Generic;

namespace Arnis.Core
{
    public class Workspace
    {
        public string ApiKey { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Owners { get; set; }

        public List<Solution> Solutions { get; set; }
        public List<string> Logs { get; set; }
    }

    public enum Source {
        GAC,
        NUGET,
        NA
    }
}