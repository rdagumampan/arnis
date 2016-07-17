using Newtonsoft.Json;

namespace Arnis.Documents
{
    public class DependencyHit : DocumentBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hits")]
        public int Hits { get; set; }
    }
}