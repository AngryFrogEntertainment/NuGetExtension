using AngryFrog.NuGetToolsExtension.Configuration;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Windows;

namespace AngryFrog.NuGetToolsExtension.Windows
{
    /// <summary>
    /// Interaktionslogik für ConfigDialog.xaml
    /// </summary>
    public partial class ConfigDialog : DialogWindow
    {
        private NuGetConfig config;
        private readonly IServiceProvider serviceProvider;

        // Use this constructor for minimize and maximize buttons and no F1 Help.
        public ConfigDialog(IServiceProvider serviceProvider)
        {
            this.HasMaximizeButton = false;
            this.HasMinimizeButton = false;
            this.serviceProvider = serviceProvider;
            InitializeComponent();
            config = NuGetConfigurator.LoadConfig();
            initializeFields();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void initializeFields()
        {
            txtFeed.Text = config.FeedConfig.Feed;
            txtKey.Text = config.FeedConfig.PublicKey;
	        chkReferences.IsChecked = config.AreReferencesIncluded;
	        chkSymbols.IsChecked = config.AreSymbolsIncluded;
	        txtDir.Text = config.DefaultOutputDirectory;
	        chkOutput.IsChecked = config.UseDefaultOutput;
	        chkBuild.IsChecked = config.IsBuildEnabled;

	        if (config.UseDefaultOutput)
			{
				txtDir.IsEnabled = true;
				btnBrowse.IsEnabled = true;
			}
			else
			{
				txtDir.IsEnabled = false;
				btnBrowse.IsEnabled = false;
			}
		}

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            config.FeedConfig.Feed = txtFeed.Text;
            config.FeedConfig.PublicKey = txtKey.Text;
	        config.AreReferencesIncluded = chkReferences.IsChecked ?? false;
			config.AreSymbolsIncluded = chkSymbols.IsChecked ?? false;
			config.UseDefaultOutput = chkOutput.IsChecked ?? false;
			config.DefaultOutputDirectory = txtDir.Text;
			config.IsBuildEnabled = chkBuild.IsChecked ?? false;

	        try
	        {
		        NuGetConfigurator.SaveConfig(config);

				VsShellUtilities.ShowMessageBox(
					serviceProvider,
					"Config saved.",
					"Configure NuGetTools",
					OLEMSGICON.OLEMSGICON_INFO,
					OLEMSGBUTTON.OLEMSGBUTTON_OK,
					OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

				Close();
			}
	        catch (Exception ex)
	        {
		        VsShellUtilities.ShowMessageBox(
			        serviceProvider,
			        "Could not save config." + Environment.NewLine + ex.Message,
			        "Configure NuGetTools",
			        OLEMSGICON.OLEMSGICON_INFO,
			        OLEMSGBUTTON.OLEMSGBUTTON_OK,
			        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
	        }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();

			var result = dialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK)
			{
				txtDir.Text = dialog.SelectedPath;
			}
		}

		private void chkOutput_Checked(object sender, RoutedEventArgs e)
		{
			txtDir.IsEnabled = true;
			btnBrowse.IsEnabled = true;
		}

		private void chkOutput_Unchecked(object sender, RoutedEventArgs e)
		{
			txtDir.IsEnabled = false;
			btnBrowse.IsEnabled = false;
		}
	}
}
