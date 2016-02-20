using System.Collections.Generic;
using Arnis.Core;

namespace Arnis.Api
{
    public class Workspace
    {
        public string ApiKey { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Owners { get; set; }
        public List<Solution> Solutions { get; set; }
    }
}