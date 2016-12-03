using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace AngryFrog.NuGetToolsExtension.Configuration
{
    public static class NuGetConfigurator
    {
        private const string configFileName = "NuGetExtension.config";

        public static void SaveConfig(NuGetConfig config)
        {
			var path = getConfigPath();
			var jsonConfig = JsonConvert.SerializeObject(config);
            File.WriteAllText(path, jsonConfig);
        }

        public static NuGetConfig LoadConfig()
        {
            NuGetConfig config = null;
	        var path = getConfigPath();

			if (File.Exists(path))
            {
                var jsonConfig = File.ReadAllText(path);
                config = JsonConvert.DeserializeObject<NuGetConfig>(jsonConfig);
            }
            else
            {
                config = new NuGetConfig();

                File.WriteAllText(path, JsonConvert.SerializeObject(config));
            }

            return config;
        }

	    private static string getConfigPath()
	    {
			var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var configDir = Path.Combine(appDataDir, "NuGetToolsExtension");

		    if (!Directory.Exists(configDir))
		    {
			    Directory.CreateDirectory(configDir);
		    }

		    return Path.Combine(configDir, configFileName);
		}
    }
}
