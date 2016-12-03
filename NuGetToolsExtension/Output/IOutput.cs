namespace AngryFrog.NuGetToolsExtension.Output
{
    public interface IOutput
    {
        void Write(string text);
        void WriteLine(string text);
        void WriteError(string text);
        void WriteErrorLine(string text);
        void WriteWarning(string text);
        void WriteWarningLine(string text);
    }
}
