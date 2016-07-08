using System.Collections.Generic;
using Newtonsoft.Json;

namespace Arnis.Documents
{
    public class Solution : DocumentBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("projects")]
        public List<Project> Projects { get; set; } = new List<Project>();

        [JsonProperty("dependencies")]
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();
    }
}