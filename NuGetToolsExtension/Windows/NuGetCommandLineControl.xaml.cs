//------------------------------------------------------------------------------
// <copyright file="NuGetToolWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace AngryFrog.NuGetToolsExtension.Windows
{
    using System;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetToolWindowControl"/> class.
        /// </summary>
        public NuGetCommandLineControl()
        {
            this.InitializeComponent();
        }

        private void txtIn_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                TextRange tr = new TextRange(txtOut.Document.ContentEnd, txtOut.Document.ContentEnd);
                tr.Text = txtIn.Text;
                //tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                txtIn.Text = "";
            }
        }
    }
}