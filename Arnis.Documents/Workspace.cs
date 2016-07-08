using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Arnis.Documents
{
    public class Workspace : DocumentBase
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0, 4);

        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("owners")]
        public List<string> Owners { get; set; }

        [JsonProperty("solutions")]
        public List<Solution> Solutions { get; set; } = new List<Solution>();

        [JsonProperty("logs")]
        public List<string> Logs { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [JsonProperty("dateUpdated")]
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    }
}