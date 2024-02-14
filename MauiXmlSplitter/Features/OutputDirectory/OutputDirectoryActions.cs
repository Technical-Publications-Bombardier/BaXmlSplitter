namespace MauiXmlSplitter;

public class SelectOutputDirectoryAction(string outputDirectory)
{
    public string OutputDirectory { get; } = outputDirectory;
}

public class OutputDirectoryErrorAction(Exception exception)
{
    public Exception Exception { get; } =
        exception ?? throw new ArgumentNullException(nameof(exception)); // for syntax errors
}

public class OutputDirectoryCreatedAction(DirectoryInfo directory)
{
    public DirectoryInfo OutputDirectory { get; } = directory;
}