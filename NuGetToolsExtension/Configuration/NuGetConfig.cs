using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AngryFrog.NuGetToolsExtension.Configuration
{
    public class NuGetConfig
    {
        public NuGetConfig()
        {
            FeedConfig = new FeedConfig();
            VerbosityLevel = "detailed";
            AreSymbolsIncluded = true;
            AreReferencesIncluded = true;
	        IsBuildEnabled = true;
        }

        [JsonProperty("feedConfig")]
        public FeedConfig FeedConfig { get; set; }
        [JsonProperty("verbosityLevel")]
        public string VerbosityLevel { get; set; }
        [JsonProperty("areSymbolsIncluded")]
        public bool AreSymbolsIncluded { get; set; }
        [JsonProperty("areReferencesIncluded")]
        public bool AreReferencesIncluded { get; set; }
		[JsonProperty("isBuildEnabled")]
		public bool IsBuildEnabled { get; set; }
		[JsonProperty("defaultOutputDirectory")]
        public string DefaultOutputDirectory { get; set; }
		[JsonProperty("useDefaultOutput")]
		public bool UseDefaultOutput { get; set; }
		// Insert other stuff here
	}
}
