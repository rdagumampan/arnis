using Newtonsoft.Json;

namespace Arnis.Documents
{
    public class DependencyHit : DocumentBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("hits")]
        public int Hit { get; set; }
    }
}