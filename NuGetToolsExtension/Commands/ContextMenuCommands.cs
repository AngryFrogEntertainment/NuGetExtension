//------------------------------------------------------------------------------
// <copyright file="ContextMenuCommands.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using AngryFrog.NuGetToolsExtension.Extensions;
using AngryFrog.NuGetToolsExtension;
using System.IO;
using Microsoft.VisualStudio;

namespace AngryFrog.NuGetToolsExtension.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ContextMenuCommands
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int nuspecCmdId = 256;
        public const int packageCmdId = 128;
        public const int pushCmdId = 512;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("d96a0038-2a0f-456a-807f-042b7f935a3d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ContextMenuCommands(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var nuspecCommandID = new CommandID(CommandSet, nuspecCmdId);
                var nuspecMenuItem = new OleMenuCommand(this.createNuspecCallback, nuspecCommandID);
                nuspecMenuItem.BeforeQueryStatus += beforeQueryStatus;
                var packageCommandID = new CommandID(CommandSet, packageCmdId);
                var packageMenuItem = new OleMenuCommand(this.createPackageCallback, packageCommandID);
                packageMenuItem.BeforeQueryStatus += beforeQueryStatus;
                var pushCommandID = new CommandID(CommandSet, pushCmdId);
                var pushMenuItem = new OleMenuCommand(this.pushPackageCallback, pushCommandID);
                packageMenuItem.BeforeQueryStatus += beforePushQueryStatus;

                commandService.AddCommand(nuspecMenuItem);
                commandService.AddCommand(packageMenuItem);
                commandService.AddCommand(pushMenuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ContextMenuCommands Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new ContextMenuCommands(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void createNuspecCallback(object sender, EventArgs e)
        {
            var project = ServiceProvider.GetSelectedFileFromSolutionExplorer<Project>();

            if (project != null)
            {
                var nuGetToolsPackage = package as NuGetToolsPackage;

                if (nuGetToolsPackage != null)
                {
                    nuGetToolsPackage.CreateNuspec(project.FileName);
                }
                else
                {
                    VsShellUtilities.ShowMessageBox(
                        ServiceProvider,
                        "Creation of NuSpec file failed! An unexpected error occured.",
                        "Create NuSpec",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void createPackageCallback(object sender, EventArgs e)
        {
            var project = ServiceProvider.GetSelectedFileFromSolutionExplorer<Project>();

            if (project != null)
            {
                var nuGetToolsPackage = package as NuGetToolsPackage;

                if (nuGetToolsPackage != null)
                {
                    var assemblyName = project.Properties.Item("AssemblyName").Value.ToString();
                    nuGetToolsPackage.CreatePackage(project.FullName, assemblyName);
                }
                else
                {
                    VsShellUtilities.ShowMessageBox(
                        ServiceProvider,
                        "Creation of package file failed! An unexpected error occured.",
                        "Create Package",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void pushPackageCallback(object sender, EventArgs e)
        {
            var project = ServiceProvider.GetSelectedFileFromSolutionExplorer<object>();

            if (project != null)
            {
                var nuGetToolsPackage = package as NuGetToolsPackage;

                if (nuGetToolsPackage != null)
                {
                    //nuGetToolsPackage.CreatePackage(project.FullName);
                }
                else
                {
                    VsShellUtilities.ShowMessageBox(
                        ServiceProvider,
                        "Push of package file failed! An unexpected error occured.",
                        "Create Package",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
        }

        private void beforeQueryStatus(object sender, EventArgs e)
        {
            var menuCmd = sender as OleMenuCommand;

            if (menuCmd != null)
            {
                var project = ServiceProvider.GetSelectedFileFromSolutionExplorer<Project>();

                menuCmd.Visible = project != null;
            }
        }

        private void beforePushQueryStatus(object sender, EventArgs e)
        {
            var menuCmd = sender as OleMenuCommand;

            //if (menuCmd != null)
            //{
            //    var project = ServiceProvider.GetSelectedFileFromSolutionExplorer<Project>();

            //    menuCmd.Visible = project != null;
            //}
        }
    }
}
