using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System;
using System.Linq;
using System.Xml;
using AngryFrog.NuGetToolsExtension.Configuration;
using AngryFrog.NuGetToolsExtension.Output;

namespace AngryFrog.NuGetToolsExtension
{
	public class NuGetCommands
	{
		public FileInfo LastCreatedFile { get; private set; }

		private readonly NuGetConfig config;
		private readonly ProcessStartInfo processInfo = new ProcessStartInfo();
		private readonly IOutput output;

		public NuGetCommands(IOutput output)
		{
			LastCreatedFile = null;
			this.config = NuGetConfigurator.LoadConfig();
			processInfo.FileName = $@"{NuGetToolsPackage.ExtensionPath}\Resources\nuget.exe";
			processInfo.UseShellExecute = false;
			processInfo.RedirectStandardOutput = true;
			processInfo.RedirectStandardError = true;
			processInfo.CreateNoWindow = true;

			this.output = output;
		}

		public async Task<int> CreatePackage(string projectPath, string outputPath, string assemblyName, string version = null)
		{
			outputPath = checkOutputPath(outputPath);
			var symbols = config.AreSymbolsIncluded ? "-Symbols" : string.Empty;
			var versionOverwrite = !string.IsNullOrEmpty(version) ? $"-Version {version}" : string.Empty;
			var includeReferences = config.AreReferencesIncluded ? "-IncludeReferencedProjects" : string.Empty;
			var build = config.IsBuildEnabled ? "-Build" : "";

			output.WriteLine($"Creating package for '{Path.GetFileName(projectPath)}' at '{outputPath}'...");
			var arguments = string.Format($@"""{projectPath}"" {build} {includeReferences} {symbols} {versionOverwrite} -outputDirectory ""{outputPath}""");

            var nugetFilesBefore = Directory.GetFiles(outputPath, "*.nupkg");
            var exitCode = await RunNuget("pack", arguments);
            var nugetFilesAfter = Directory.GetFiles(outputPath, "*.nupkg");

            var newNugetFiles = nugetFilesAfter.Except(nugetFilesBefore).ToList();

            if (exitCode == 0)
            {
                if (newNugetFiles.Count == 2 && config.AreSymbolsIncluded)
                {
                    // If symbols are included, delete the other Package
                    var nugetFile = newNugetFiles.FirstOrDefault(x => !x.Contains("symbols"));
                    if (!string.IsNullOrEmpty(nugetFile))
                    {
                        File.Delete(nugetFile);
                        newNugetFiles.Remove(nugetFile);
                    }
                }

                if (newNugetFiles.Count == 1)
                {
                    LastCreatedFile = new FileInfo(newNugetFiles[0]);
                }
                else
                {
                    output.WriteWarningLine("Created NuGet package could not be identified. Package has to be pushed manually");
                } 
            }

			return exitCode;
		}

		private string checkOutputPath(string outputPath)
		{
			var outputDir = outputPath;

			if (!string.IsNullOrEmpty(config.DefaultOutputDirectory) && config.UseDefaultOutput)
			{
				outputDir = Path.GetFullPath(config.DefaultOutputDirectory);
			}

		    return outputDir;
	    }

	    public async Task<int> CreateNuspec(string projectPath, bool force = false)
        {
            output.WriteLine($"Creating nuspec file for '{Path.GetFileName(projectPath)}'...");
            var arguments = force ? "-f" : string.Empty;
            int exitCode = await RunNuget("spec", arguments, Path.GetDirectoryName(projectPath));

            return exitCode;
        }

        public async Task<int> PushPackage(string packagePath)
        {
            if (string.IsNullOrEmpty(config.FeedConfig.Feed))
            {
                throw new ArgumentException("No target feed configured!");
            }

            if (string.IsNullOrEmpty(config.FeedConfig.PublicKey))
            {
                throw new ArgumentException("No public key configured!");
            }

            // {package file} {apikey} -Source {feed}
            var arguments = $@"""{packagePath}"" {config.FeedConfig.PublicKey} -Source {config.FeedConfig.Feed}";

            return await RunNuget("push", arguments);
        }

        public async Task<int> ClearPackageCache()
        {
            output.WriteLine("Clearing Chache...");
            return await RunNuget("locals all", "-clear");
        }

        public async Task<int> RunNuget(string nugetCommand, string nugetArguments, string workingDirectory = "")
        {
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                processInfo.WorkingDirectory = workingDirectory;
            }
            processInfo.Arguments = $"{nugetCommand} {nugetArguments} -Verbosity {config.VerbosityLevel}";

            return await new TaskFactory().StartNew(() =>
            {
                var process = Process.Start(processInfo);

				try
				{
					process.OutputDataReceived += writeProcessData;
					process.ErrorDataReceived += writeProcessErrorData;
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();
					process.WaitForExit();
					output.WriteLine($"ExitCode: '{process.ExitCode}'");
				}
				catch (Exception e)
				{
					output.WriteError(e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine);
					throw;
				}

				return process.ExitCode;
            });
        }

        private void writeProcessData(object sender, DataReceivedEventArgs args)
        {
            output.WriteLine(args.Data);
        }

        private void writeProcessErrorData(object sender, DataReceivedEventArgs args)
        {
            output.WriteErrorLine(args.Data);
        }
    }
}
