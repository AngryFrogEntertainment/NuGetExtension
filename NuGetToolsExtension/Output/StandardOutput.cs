using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace AngryFrog.NuGetToolsExtension.Output
{
    public class StandardOutput : IOutput 
    {
        public static Guid PaneGuid = new Guid("0F44E2D1-F5FA-4d2d-AB30-22BE8ECD9789");
        private IVsOutputWindowPane customPane;
        private Window window;

        public StandardOutput()
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            string customTitle = "NuGet Tools Output";
            outWindow.CreatePane(ref PaneGuid, customTitle, 1, 1);
            outWindow.GetPane(ref PaneGuid, out customPane);

            DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));
            window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
        }

        public void Write(string text)
        {
            activateOutputWindow();
            customPane.OutputString(text);
        }

        public void WriteLine(string text)
        {
            Write(text + Environment.NewLine);
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

        private void activateOutputWindow()
        {
            window.Activate();
            customPane.Activate();
        }
    }
}
