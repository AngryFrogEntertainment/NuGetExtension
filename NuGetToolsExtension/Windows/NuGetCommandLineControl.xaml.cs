//------------------------------------------------------------------------------
// <copyright file="NuGetToolWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace AngryFrog.NuGetToolsExtension.Windows
{
    using Output;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    /// <summary>
    /// Interaction logic for NuGetToolWindowControl.
    /// </summary>
    public partial class NuGetCommandLineControl : UserControl
    {
        private NuGetCommands nuGetCommands;
        private IOutput output;

        private List<string> lastCommandsList = new List<string>();
        private int lastCommandIndex = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetToolWindowControl"/> class.
        /// </summary>
        public NuGetCommandLineControl()
        {
            this.InitializeComponent();
            output = new RtbOutput(txtOut);
            nuGetCommands = new NuGetCommands(output);
        }

        private void txtIn_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                output.WriteLine($"nuget {txtIn.Text}:\r");

                nuGetCommands.RunNuget(txtIn.Text, string.Empty, txtDir.Text).ContinueWith(t =>
                {
                    txtIn.Dispatcher.Invoke(() => 
                    {
                        txtIn.IsEnabled = true;
                        txtIn.Focus();
                    });
                });

                txtIn.IsEnabled = false;
                lastCommandsList.Add(txtIn.Text);
                lastCommandIndex++;
                txtIn.Text = "";
            }
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

        private void txtIn_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up)
            {
                if (lastCommandIndex > 0)
                {
                    lastCommandIndex--;
                    txtIn.Text = lastCommandsList[lastCommandIndex];
                }
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                if (lastCommandIndex < lastCommandsList.Count - 1)
                {
                    lastCommandIndex++;
                    txtIn.Text = lastCommandsList[lastCommandIndex];
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            lastCommandsList.Clear();
            lastCommandIndex = 0;
            var range = new TextRange(txtOut.Document.ContentStart, txtOut.Document.ContentEnd);
            range.Text = "";
        }
    }
}