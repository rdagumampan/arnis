using System.Collections.Generic;
using Newtonsoft.Json;

namespace Arnis.Documents
{
    public class Project: DocumentBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("dependencies")]
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();
    }
}