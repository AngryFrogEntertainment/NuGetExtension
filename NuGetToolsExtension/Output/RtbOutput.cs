using System;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Documents;

namespace AngryFrog.NuGetToolsExtension.Output
{
    public class RtbOutput : IOutput
    {
        private RichTextBox rtb;

        public RtbOutput(RichTextBox rtb)
        {
            this.rtb = rtb;
        }

        public void Write(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                rtb.Dispatcher.Invoke(() =>
                {
                    TextRange tr = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
                    tr.Text = text;
                });
            }
        }

        public void WriteLine(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Write(text + "\r");
            }
        }

        public void WriteError(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Write("Error: " + text);
            }
        }

        public void WriteErrorLine(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                WriteLine("Error: " + text);
            }
        }

        public void WriteWarning(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Write("Warning: " + text);
            }
        }

        public void WriteWarningLine(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                WriteLine("Warning: " + text);
            }
        }
    }
}
