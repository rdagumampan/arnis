using Newtonsoft.Json;

namespace Arnis.Documents
{
    public abstract class VersionInfo: DocumentBase
    {
        [JsonProperty("major")]
        public int Major { get; set; }

        [JsonProperty("minor")]
        public int Minor { get; set; }

        [JsonProperty("build")]
        public int Build { get; set; }

        [JsonProperty("revision")]
        public int Revision { get; set; }
    }
}