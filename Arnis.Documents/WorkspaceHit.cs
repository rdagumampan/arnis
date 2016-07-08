    using System;
using Newtonsoft.Json;

namespace Arnis.Documents
{
    public class WorkspaceHit : DocumentBase
    {
        [JsonProperty("workspaceId")]
        public string WorkspaceId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("hits")]
        public int Hit { get; set; }
    }
}