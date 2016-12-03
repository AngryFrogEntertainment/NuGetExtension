//------------------------------------------------------------------------------
// <copyright file="NuGetToolsPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Xml;
using AngryFrog.NuGetToolsExtension.Windows;
using AngryFrog.NuGetToolsExtension.Commands;
using AngryFrog.NuGetToolsExtension.Output;
using EnvDTE;

namespace AngryFrog.NuGetToolsExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(NuGetToolsPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(NuGetCommandLine))]
    public sealed class NuGetToolsPackage : Package
    {
        private OleMenuCommandService commandService;
		private IOutput output = new StandardOutput();

        public static string ExtensionPath { get; private set; }

        /// <summary>
        /// NuGetToolsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "c65f4c80-d68a-4dbb-a8da-328a3f29e6f8";
        
        //commandIds
        public const int CreatePackageCmdId = 0x0101;
        public const int ClearCacheCmdId = 0x0102;
        public const int PushPackageCmdId = 0x0103;
        public const int CreateNuspecCmdId = 0x0104;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetToolsPackage"/> class.
        /// </summary>
        public NuGetToolsPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            commandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            ConfigCommand.Initialize(this);
            initializeCreatePackageCommand();
            initializeClearCacheCommand();
            initializePushPackageCommand();
            initializeCreateNuspecCommand();
            NuGetCommandLineCommand.Initialize(this);
            ContextMenuCommands.Initialize(this);

            ExtensionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        #region Command Initializations

        private void initializeCreatePackageCommand()
        {
            if (commandService != null)
            {
                var menuCommandID = new CommandID(ConfigCommand.CommandSet, CreatePackageCmdId);
                var menuItem = new MenuCommand(createPackageCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        private void initializeClearCacheCommand()
        {
            if (commandService != null)
            {
                var menuCommandID = new CommandID(ConfigCommand.CommandSet, ClearCacheCmdId);
                var menuItem = new MenuCommand(clearCacheCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        private void initializePushPackageCommand()
        {
            if (commandService != null)
            {
                var menuCommandID = new CommandID(ConfigCommand.CommandSet, PushPackageCmdId);
                var menuItem = new MenuCommand(pushPackageCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        private void initializeCreateNuspecCommand()
        {
            if (commandService != null)
            {
                var menuCommandID = new CommandID(ConfigCommand.CommandSet, CreateNuspecCmdId);
                var menuItem = new MenuCommand(createNuspecCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        #endregion

        #region Command Callbacks

        private void createPackageCallback(object sender, EventArgs e)
        {
            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "VS Project (.csproj)|*.csproj"; // Filter files by extension

            // Show open file dialog box
            var result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                var fileName = dlg.FileName;
                var doc = new XmlDocument();
                doc.Load(fileName);
                var assemblyNames = doc.GetElementsByTagName("AssemblyName");
                var assemblyName = string.Empty;

                if (assemblyNames.Count > 0)
                {
                    assemblyName = assemblyNames[0].InnerText;
                }

                CreatePackage(fileName, assemblyName);
            }
        }

        private void clearCacheCallback(object sender, EventArgs e)
        {
            var nugetCommands = new NuGetCommands(output);

            nugetCommands.ClearPackageCache().ContinueWith(t =>
            {
				try
				{
					var code = t.Result;
					if (code != 0)
					{
						VsShellUtilities.ShowMessageBox(
							this,
							"Clear NuGet cache failed! See output window for further information.",
							"Clear NuGet Cache",
							OLEMSGICON.OLEMSGICON_WARNING,
							OLEMSGBUTTON.OLEMSGBUTTON_OK,
							OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
					}
				}
				catch (Exception ex)
				{
					VsShellUtilities.ShowMessageBox(
							this,
							"Clear NuGet cache failed!" + ex.Message + "." + Environment.NewLine + "See output window for further information.",
							"Clear NuGet Cache",
							OLEMSGICON.OLEMSGICON_WARNING,
							OLEMSGBUTTON.OLEMSGBUTTON_OK,
							OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

					output.WriteError(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
				}
            });
        }

        private void pushPackageCallback(object sender, EventArgs e)
        {
            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "NuGet Packages (.nupkg)|*.nupkg"; // Filter files by extension
            //dlg.Multiselect = true; allow multiselect later

            // Show open file dialog box
            var result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                pushPackage(dlg.FileName);
            }
        }

        private void createNuspecCallback(object sender, EventArgs e)
        {
            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "VS Project (.csproj)|*.csproj"; // Filter files by extension

            // Show open file dialog box
            var result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                CreateNuspec(dlg.FileName);
            }
        }

        #endregion

        public void CreateNuspec(string projectFile)
        {
			try
			{
				var dir = Path.GetDirectoryName(projectFile);
				var nuspecs = Directory.GetFiles(dir, "*.nuspec");

				if (nuspecs.Length > 0)
				{
					var decision = VsShellUtilities.ShowMessageBox(
							this,
							"This project already contains an nuspec file. Overwrite?",
							"Create NuSpec",
							OLEMSGICON.OLEMSGICON_WARNING,
							OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
							OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

					if (decision == 6)
					{
						createNuspec(projectFile, true);
					}
				}
				else
				{
					createNuspec(projectFile);
				}
			}
			catch (Exception e)
			{
				displayException(e);
			} 
        }

        private void createNuspec(string projectFile, bool force = false)
        {
            var nugetCommands = new NuGetCommands(output);
            nugetCommands.CreateNuspec(projectFile, force).ContinueWith(t =>
            {
				try
				{
					var code = t.Result;
					if (code != 0)
					{
						VsShellUtilities.ShowMessageBox(
							this,
							"Creation of nuspec file failed! See output window for further information.",
							"Create NuSpec",
							OLEMSGICON.OLEMSGICON_WARNING,
							OLEMSGBUTTON.OLEMSGBUTTON_OK,
							OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
					}
					else
					{
						var nuspecFile = Path.GetFileNameWithoutExtension(projectFile) + ".nuspec";
						var dir = Path.GetDirectoryName(projectFile);

						VsShellUtilities.OpenDocument(this, Path.Combine(dir, nuspecFile));
					}
				}
				catch (Exception ex)
				{
					VsShellUtilities.ShowMessageBox(
							this,
							"Creation of nuspec file failed!" + ex.Message + "." + Environment.NewLine + "See output window for further information.",
							"Create NuSpec",
							OLEMSGICON.OLEMSGICON_WARNING,
							OLEMSGBUTTON.OLEMSGBUTTON_OK,
							OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

					output.WriteError(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
				}
			});
        }

        public void CreatePackage(string projectFile, string assemblyName)
        {
			try
			{
				var projectDir = Path.GetDirectoryName(projectFile);
				new VersionDialog(this).Show(projectDir, (version) =>
				{
					var nugetCommands = new NuGetCommands(output);
					nugetCommands.CreatePackage(projectFile, projectDir, assemblyName, version).ContinueWith(t =>
					{
						try
						{
							var code = t.Result;
							if (code != 0)
							{
								VsShellUtilities.ShowMessageBox(
									this,
									"Creation of package file failed! See output window for further information.",
									"Create Package",
									OLEMSGICON.OLEMSGICON_WARNING,
									OLEMSGBUTTON.OLEMSGBUTTON_OK,
									OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
							}
							else
							{
								if (nugetCommands.LastCreatedFile != null)
								{
									var decision = VsShellUtilities.ShowMessageBox(
										this,
										"Package created. Push it?",
										"Create Package",
										OLEMSGICON.OLEMSGICON_QUERY,
										OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
										OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

									if (decision == 6)
									{
										pushPackage(nugetCommands.LastCreatedFile.FullName);
									}
								}
							}
						}
						catch (Exception ex)
						{
							VsShellUtilities.ShowMessageBox(
									this,
									"Creation of package file failed!" + ex.Message + "." + Environment.NewLine + "See output window for further information.",
									"Create Package",
									OLEMSGICON.OLEMSGICON_WARNING,
									OLEMSGBUTTON.OLEMSGBUTTON_OK,
									OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

							output.WriteError(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
						}
					});
				});
			}
			catch (Exception e)
			{
				displayException(e);
			}
        }

        private void pushPackage(string packageFile)
        {
            // Open document
            var nugetCommands = new NuGetCommands(output);
            nugetCommands.PushPackage(packageFile).ContinueWith(t =>
            {
				try
				{
					var code = t.Result;
					if (code != 0)
					{
						VsShellUtilities.ShowMessageBox(
							this,
							"Push of package failed! See output window for further information.",
							"Push package",
							OLEMSGICON.OLEMSGICON_WARNING,
							OLEMSGBUTTON.OLEMSGBUTTON_OK,
							OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
					}
					else
					{
						var decision = VsShellUtilities.ShowMessageBox(
									this,
									"Package pushed. Delete it?",
									"Delete Package",
									OLEMSGICON.OLEMSGICON_QUERY,
									OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
									OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

						if (decision == 6)
						{
							File.Delete(packageFile);
						}
					}
				}
				catch (Exception ex)
				{
					VsShellUtilities.ShowMessageBox(
							this,
							"Push of package failed!" + ex.Message + "." + Environment.NewLine + "See output window for further information.",
							"Push Package",
							OLEMSGICON.OLEMSGICON_WARNING,
							OLEMSGBUTTON.OLEMSGBUTTON_OK,
							OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

					output.WriteError(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
				}
			});
        }

	    private void displayException(Exception e)
	    {
			var decision = VsShellUtilities.ShowMessageBox(
								this,
								e.Message,
								"Error",
								OLEMSGICON.OLEMSGICON_CRITICAL,
								OLEMSGBUTTON.OLEMSGBUTTON_OK,
								OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
		}

	    #endregion
    }
}
