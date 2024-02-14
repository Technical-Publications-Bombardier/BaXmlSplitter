using MauiXmlSplitter.Models;

namespace MauiXmlSplitter.XmlFileActions;

/// <summary>
/// Actions for the XML file selection.
/// </summary>
public class SanitizeXmlAction(Stream xmlStream, Dictionary<int, string> lookupEntities)
{
    /// <summary>
    /// Gets the XML stream.
    /// </summary>
    /// <value>
    /// The XML stream.
    /// </value>
    public Stream XmlStream { get; } = xmlStream;
    /// <summary>
    /// Gets the lookup entities.
    /// </summary>
    /// <value>
    /// The lookup entities.
    /// </value>
    public Dictionary<int, string> LookupEntities { get; } = lookupEntities;
}

/// <summary>
/// Complete the XML sanitization action.
/// </summary>
public class CompleteSanitizationAction(Stream sanitizedXml)
{
    /// <summary>
    /// Gets the sanitized XML.
    /// </summary>
    /// <value>
    /// The sanitized XML.
    /// </value>
    public Stream SanitizedXml { get; } = sanitizedXml;
}

/// <summary>
/// Pick XML file action.
/// </summary>
public class PickFileAction(string filePath)
{
    /// <summary>
    /// Gets the file path.
    /// </summary>
    /// <value>
    /// The file path.
    /// </value>
    public string FilePath { get; } = filePath;
}

/// <summary>
/// Action for when the XML file has been picked.
/// </summary>
public class FilePickedAction(string content)
{
    /// <summary>
    /// Gets the content of the XML.
    /// </summary>
    /// <value>
    /// The content of the XML.
    /// </value>
    public string XmlContent { get; } = content;
}

/// <summary>
/// The XML loading failed action.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FilePickFailedAction"/> class.
/// </remarks>
/// <param name="exception">The error.</param>
public class FilePickFailedAction(Exception exception)
{
    /// <summary>
    /// Gets the error message.
    /// </summary>
    /// <value>
    /// The error message.
    /// </value>
    public Exception Exception { get; } = exception;
}
/// <summary>
/// Action for updating the XML document.
/// </summary>
public class UpdateDocumentAction(BaXmlDocument document)
{
    /// <summary>
    /// Gets the XML.
    /// </summary>
    /// <value>
    /// The XML.
    /// </value>
    public BaXmlDocument Xml { get; } = document;
}

/// <summary>
/// Action for when the XML file loading fails.
/// </summary>
public class FileLoadFailedAction
{
}
