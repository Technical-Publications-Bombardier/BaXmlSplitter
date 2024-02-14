using Fluxor;
namespace MauiXmlSplitter
{
    [FeatureState]
    public class OutputDirectoryState
    {
        public OutputDirectoryState(DirectoryInfo directory, Exception? exception = default)
        {
            OutputDirectory = directory;
            Exception = exception;
        }
        public OutputDirectoryState()
        {
            OutputDirectory = new(".");
            Exception = default;
        }

        public DirectoryInfo OutputDirectory { get; }
        public Exception? Exception { get; }
    }
}
