using Fluxor;
using MauiXmlSplitter.XmlFileActions;

namespace MauiXmlSplitter;

/// <summary>
///     <see cref="XmlFileState" /> reducers.
/// </summary>
public static class XmlFileReducers
{
    /// <summary>
    ///     Reduces the update document action.
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="action">The action.</param>
    /// <returns>XML File State</returns>
    [ReducerMethod]
    public static XmlFileState ReduceUpdateDocumentAction(XmlFileState state, UpdateDocumentAction action)
    {
        return new XmlFileState(state.FilePath, action.Xml);
    }

    /// <summary>
    ///     Performs the Sanitize XML Action
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="action">The action.</param>
    /// <returns>SanitizeXmlAction</returns>
    [ReducerMethod]
    public static XmlFileState ReduceSanitizeXmlAction(XmlFileState state, SanitizeXmlAction action)
    {
        throw new NotImplementedException("No reducer for SanitizeXmlAction");
    }

    /// <summary>
    ///     Performs the Complete Sanitization Action
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="action">The action.</param>
    /// <returns>CompleteSanitizationAction</returns>
    [ReducerMethod]
    public static XmlFileState ReduceCompleteSanitizationAction(XmlFileState state, CompleteSanitizationAction action)
    {
        throw new NotImplementedException("No reducer for CompleteSanitizationAction");
    }

    /// <summary>
    ///     Performs the Pick File Action
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="action">The action.</param>
    /// <returns>PickFileAction</returns>
    [ReducerMethod]
    public static XmlFileState ReducePickFileAction(XmlFileState state, PickFileAction action)
    {
        throw new NotImplementedException("No reducer for PickFileAction");
    }

    /// <summary>
    ///     Performs the File Picked Action
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="action">The action.</param>
    /// <returns>FilePickedAction</returns>
    [ReducerMethod]
    public static XmlFileState ReduceFilePickedAction(XmlFileState state, FilePickedAction action)
    {
        throw new NotImplementedException("No reducer for FilePickedAction");
    }

    /// <summary>
    ///     Performs the File Load Failed Action
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="action">The action.</param>
    /// <returns>FileLoadFailedAction</returns>
    [ReducerMethod]
    public static XmlFileState ReduceFileLoadFailedAction(XmlFileState state, FileLoadFailedAction action)
    {
        throw new NotImplementedException("No reducer for FileLoadFailedAction");
    }
    /// <summary>
    /// Performs the FilePickFailedAction
    /// </summary>
    /// <param name="state">The state.</param>
    /// <param name="action">The action.</param>
    /// <returns>FilePickFailedAction</returns>
    [ReducerMethod]
    public static XmlFileState ReduceFilePickFailedAction(XmlFileState state, FilePickFailedAction action)
    {
        throw new NotImplementedException("No reducer for FilePickFailedAction");
    }
}