using System;
using Newtonsoft.Json;

namespace Arnis.Documents
{
    public class Account : DocumentBase
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0,4);

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [JsonProperty("dateUpdated")]
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    }
}