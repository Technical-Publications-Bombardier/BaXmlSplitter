using System.Xml;
using Fluxor;
using MauiXmlSplitter.Models;
using MauiXmlSplitter.XmlFileActions;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using IDispatcher = Fluxor.IDispatcher;

namespace MauiXmlSplitter.Features.XmlFile;

/// <summary>
/// Effects for the XML file state.
/// </summary>
public class XmlFileEffects(ILogger<XmlFileEffects> logger)
{
    /// <summary>
    /// The valid characters
    /// </summary>
    private const string ValidCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789~#`\t*';&@_\\,!=:\"<>/+- ?.{}()[]àâçéèêëîïôùûüÿñÀÂÄÇÉÈÊËÎÏÔÙÛÜŸÑ";

    /// <summary>
    /// The timeout
    /// </summary>
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15.0);
    /// <summary>
    /// Handles the pick file action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="dispatcher">The dispatcher.</param>
    [EffectMethod]
    public async Task HandlePickFileAction(XmlFileActions.PickFileAction action, IDispatcher dispatcher)
    {
        var cts = new CancellationTokenSource(DefaultTimeout);
        try
        {
            var xmlContent = await File.ReadAllTextAsync(action.FilePath, cts.Token).ConfigureAwait(false);
            dispatcher.Dispatch(new XmlFileActions.FilePickedAction(xmlContent));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning("XML load operation cancelled: {Message}, {InnerException}", ex.Message, ex.InnerException?.Message);
            // Optionally, dispatch an action to update the state to reflect the error
            dispatcher.Dispatch(new XmlFileActions.FileLoadFailedAction());
        }
    }
    /// <summary>
    /// Handles the file picked action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="dispatcher">The dispatcher.</param>
    [EffectMethod]
    public async Task HandleFilePickedAction(XmlFileActions.FilePickedAction action, IDispatcher dispatcher)
    {
        BaXmlDocument document = new();
        try
        {
            await Task.Run(() => document.Load(action.XmlContent)).ConfigureAwait(false);
            dispatcher.Dispatch(new XmlFileActions.UpdateDocumentAction(document));
        }
        catch (XmlException ex)
        {
            logger.LogError("Error parsing XML: {Message}", ex.Message);
            // Dispatch an action to update the state to reflect the error
            dispatcher.Dispatch(new XmlFileActions.FileLoadFailedAction());
        }
        catch (ArgumentNullException ex)
        {
            logger.LogError("Null argument: {Message}", ex.Message);
            // Dispatch an action to update the state to reflect the error
            dispatcher.Dispatch(new XmlFileActions.FileLoadFailedAction());
        }
        catch (IOException ex)
        {
            logger.LogError("IO error: {Message}", ex.Message);
            // Dispatch an action to update the state to reflect the error
            dispatcher.Dispatch(new XmlFileActions.FileLoadFailedAction());
        }
    }
    /// <summary>
    /// Handles the sanitize XML action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="dispatcher">The dispatcher.</param>
    [EffectMethod]
    public async Task HandleSanitizeXmlAction(SanitizeXmlAction action, IDispatcher dispatcher)
    {
        var outputStream = new MemoryStream();
        await using var writer = new StreamWriter(outputStream, leaveOpen: true);
        using var reader = new StreamReader(action.XmlStream);

        while (reader.Peek() >= 0)
        {
            var c = (char)reader.Read();
            if (ValidCharacters.Contains(c, StringComparison.OrdinalIgnoreCase))
                await writer.WriteAsync(c);
            else if (action.LookupEntities.TryGetValue(c, out var entity))
                await writer.WriteAsync(entity);
            else
                logger.LogWarning("Unrecognized XML character entity: '{Character}' (hex: {Hex})", c, Convert.ToByte(c).ToString("x2"));
        }

        await writer.FlushAsync();
        outputStream.Position = 0;

        // Dispatch action to update state
        dispatcher.Dispatch(new CompleteSanitizationAction(outputStream));
    }
}