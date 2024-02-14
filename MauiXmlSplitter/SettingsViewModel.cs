using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BlazorBootstrap;
using BlazorContextMenu;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor;
using MauiXmlSplitter.Models;
using MauiXmlSplitter.Shared;
using Microsoft.Extensions.Logging;
using IDispatcher = Fluxor.IDispatcher;
namespace MauiXmlSplitter;

/// <summary>
///     View model for XML Splitter.
/// </summary>
/// <seealso cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject" />
public partial class SettingsViewModel(
    ConcurrentDictionary<DateTime, LogRecord> logs,
    ILogger<SettingsViewModel> logger,
    ModalService modalService,
    IState<SettingsViewModel> settingsState
    )
    : ObservableObject, IDisposable, IAsyncDisposable

{


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