using System.Collections.Generic;
using Newtonsoft.Json;

namespace Arnis.Documents
{
    public abstract class DependencyInfo: DocumentBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("authors")]
        public List<string> Authors { get; set; }

        [JsonProperty("owners")]
        public List<string> Owners { get; set; }

        [JsonProperty("licenseUrl")]
        public string LicenseUrl { get; set; }

        [JsonProperty("projectUrl")]
        public string ProjectUrl { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("version")]
        public VersionInfo Version { get; set; }

        [JsonProperty("dependencies")]
        public List<DependencyInfo> Dependencies { get; } = new List<DependencyInfo>();
    }
}