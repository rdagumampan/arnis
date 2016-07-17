    using System;
using Newtonsoft.Json;

namespace Arnis.Documents
{
    public class WorkspaceHit : DocumentBase
    {
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("workspace")]
        public string Workspace { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hits")]
        public int Hits { get; set; }
    }
}