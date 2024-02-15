using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using BlazorBootstrap;
using BlazorContextMenu;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiXmlSplitter.Models;
using MauiXmlSplitter.Shared;
using Microsoft.Extensions.Logging;
namespace MauiXmlSplitter;

/// <summary>
///     View model for XML Splitter.
/// </summary>
/// <seealso cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject" />
public partial class SettingsViewModel(
    ConcurrentDictionary<DateTime, LogRecord> logs,
    ILogger<SettingsViewModel> logger,
    ModalService modalService
    )
    : ObservableObject, IDisposable, IAsyncDisposable

{
    public string? HcpSecret { get; set; }
    /// <summary>
    /// Gets or sets the culture.
    /// </summary>
    /// <value>
    /// The culture.
    /// </value>
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize( this );
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
        GC.SuppressFinalize( this );
    }
}