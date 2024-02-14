using Fluxor;
using MauiXmlSplitter.Models;

namespace MauiXmlSplitter;

/// <summary>
/// XML file state.
/// </summary>
[FeatureState]
public class XmlFileState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlFileState"/> class.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="document">The document.</param>
    public XmlFileState(string filePath, BaXmlDocument document)
    {
        FilePath = filePath;
        Document = document;
        Loading = true;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlFileState"/> class.
    /// </summary>
    public XmlFileState()
    {
        FilePath = string.Empty;
        Document = new BaXmlDocument { ResolveEntities = false };
        Loading = false;
    }

    /// <summary>
    /// Gets the file path.
    /// </summary>
    /// <value>
    /// The file path.
    /// </value>
    public string FilePath { get; }
    /// <summary>
    /// Gets the document.
    /// </summary>
    /// <value>
    /// The document.
    /// </value>
    public BaXmlDocument Document { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="XmlFileState"/> is loading.
    /// </summary>
    /// <value>
    ///   <c>true</c> if loading; otherwise, <c>false</c>.
    /// </value>
    public bool Loading { get;  }
}