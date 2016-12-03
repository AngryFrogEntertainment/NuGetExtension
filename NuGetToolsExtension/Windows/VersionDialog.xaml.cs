using AngryFrog.NuGetToolsExtension.Configuration;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AngryFrog.NuGetToolsExtension.Windows
{
    /// <summary>
    /// Interaktionslogik für ConfigDialog.xaml
    /// </summary>
    public partial class VersionDialog : DialogWindow
    {
        private NuGetConfig config;
        private readonly IServiceProvider serviceProvider;
        private string version;
        private Action<string> okAction;

        // Use this constructor for minimize and maximize buttons and no F1 Help.
        public VersionDialog(IServiceProvider serviceProvider)
        {
            this.HasMaximizeButton = false;
            this.HasMinimizeButton = false;
            this.serviceProvider = serviceProvider;
            InitializeComponent();
            config = NuGetConfigurator.LoadConfig();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public void Show(string projectDir, Action<string> action)
        {
            txtVersion.Text = extractInfoVersionFromProject(projectDir);
            okAction = action;

            this.ShowModal();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            okAction(txtVersion.Text);
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private string extractInfoVersionFromProject(string projectDir)
        {
            var result = "1.0.0-beta";
            
            var assemblyInfoLines = File.ReadAllLines($@"{projectDir}\Properties\AssemblyInfo.cs");
            var versionLine = assemblyInfoLines.FirstOrDefault(x => x.Contains("AssemblyInformationalVersion"));

            if (!string.IsNullOrEmpty(versionLine))
            {
                var start = versionLine.IndexOf("(\"") + "(\"".Length;
                var end = versionLine.IndexOf("\")");

                result = versionLine.Substring(start, end - start);
            }

            return result;
        }

        private void txtVersion_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                okAction(txtVersion.Text);
                Close();
            }
        }
    }
}
