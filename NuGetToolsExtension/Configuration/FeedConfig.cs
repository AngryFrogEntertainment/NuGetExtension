using Newtonsoft.Json;

namespace AngryFrog.NuGetToolsExtension.Configuration
{
    public class FeedConfig
    {
        [JsonProperty("feed")]
        public string Feed { get; set; }
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }
    }
}
