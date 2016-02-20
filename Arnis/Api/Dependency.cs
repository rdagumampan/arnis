using System.Collections.Generic;

namespace Arnis.Api
{
    public class Dependency
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Owners { get; set; }

        public string LicenseUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string IconUrl { get; set; }

        public string Tags { get; set; }

        public List<Dependency> Dependencies { get; } = new List<Dependency>();  
    }
}