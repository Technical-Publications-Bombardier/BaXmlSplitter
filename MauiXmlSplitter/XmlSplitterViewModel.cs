using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BlazorBootstrap;
using BlazorContextMenu;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiXmlSplitter.Models;
using MauiXmlSplitter.Shared;
using MauiXmlSplitter.XmlFileActions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using IDispatcher = Fluxor.IDispatcher;

namespace MauiXmlSplitter;

/// <summary>
///     View model for XML Splitter.
/// </summary>
/// <seealso cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject" />
public partial class XmlSplitterViewModel(
    ConcurrentDictionary<DateTime, LogRecord> logs,
    ILogger<XmlSplitterViewModel> logger,
    ModalService modalService,
    IDispatcher dispatcher,
    Fluxor.IState<XmlFileState> xmlFileState
)
    : ObservableObject, IDisposable, IAsyncDisposable

{
    /// <summary>
    /// The units-of-work failure reason elaboration
    /// </summary>
    public static readonly Dictionary<XmlSplitter.ReasonForUowFailure, string> UowFailureReasonElaboration = new()
    {
        { XmlSplitter.ReasonForUowFailure.None, string.Empty },
        { XmlSplitter.ReasonForUowFailure.NoFileProvided, "No file provided" },
        { XmlSplitter.ReasonForUowFailure.NotPlaintextFile, "File appears not to be plaintext" },
        { XmlSplitter.ReasonForUowFailure.NotEnoughContent, "File was empty or too short" },
        { XmlSplitter.ReasonForUowFailure.DidNotMeetExpectedPattern, "File did not meet expected pattern" }
    };

    private IDispatcher Dispatcher { get; } = dispatcher;

    public void OnInputChanged(ChangeEventArgs e)
    {
        Dispatcher.Dispatch(new PickFileAction(e.Value?.ToString() ?? string.Empty));
    }
    /// <summary>
    /// The XML file type
    /// </summary>
    private static readonly FilePickerFileType XmlType = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.iOS, new[] { "public.xml" } },
            { DevicePlatform.Android, new[] { "application/xml" } },
            { DevicePlatform.WinUI, new[] { ".xml" } },
            { DevicePlatform.MacCatalyst, new[] { "public.xml" } },
            { DevicePlatform.watchOS, new[] { "public.xml" } },
            { DevicePlatform.tvOS, new[] { "public.xml" } },
            { DevicePlatform.macOS, new[] { "public.xml" } },
            { DevicePlatform.Tizen, new[] { "*/*" } }
        });

    /// <summary>
    /// The units-of-work states file type (plaintext)
    /// </summary>
    private static readonly FilePickerFileType UowType = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.iOS, new[] { "public.text" } },
            { DevicePlatform.Android, new[] { "text/plain" } },
            { DevicePlatform.WinUI, new[] { ".txt" } },
            { DevicePlatform.MacCatalyst, new[] { "public.text" } },
            { DevicePlatform.watchOS, new[] { "public.text" } },
            { DevicePlatform.tvOS, new[] { "public.text" } },
            { DevicePlatform.macOS, new[] { "public.text" } },
            { DevicePlatform.Tizen, new[] { "*/*" } }
        });

    /// <summary>
    /// The timeout
    /// </summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15.0);

    /// <summary>
    /// The XML splitter
    /// </summary>
    private readonly XmlSplitter xmlSplitter = new(logger);

    /// <summary>
    /// The loading units-of-work states file
    /// </summary>
    [ObservableProperty] private bool loadingUowStatesFile;

    /// <summary>
    /// The loading XML file
    /// </summary>
    [ObservableProperty] private bool loadingXmlFile;

    /// <summary>
    /// The select units-of-work modal
    /// </summary>
    public Modal SelectUowModal = default!;

    /// <summary>
    /// The modal service
    /// </summary>
    public ModalService ModalService = modalService;

    /// <summary>
    /// The invalid units-of-work states file modal
    /// </summary>
    public Modal InvalidUowStatesFileModal = default!;

    /// <summary>
    /// The states are selected
    /// </summary>
    public bool StatesAreSelected = false;

    /// <summary>
    /// The states
    /// </summary>
    [ObservableProperty] private IQueryable<UowState>? states;

    /// <summary>
    /// The units-of-work load success
    /// </summary>
    [ObservableProperty] private (bool Success, XmlSplitter.ReasonForUowFailure Reason) uowLoadSuccess = (false,
        XmlSplitter.ReasonForUowFailure.NoFileProvided);

    /// <summary>
    /// Gets or sets the context menu style.
    /// </summary>
    /// <value>
    /// The context menu style.
    /// </value>
    public string? ContextMenuStyle { get; set; }

    /// <summary>
    /// Gets the logs.
    /// </summary>
    /// <value>
    /// The logs.
    /// </value>
    public ConcurrentDictionary<DateTime, LogRecord> Logs { get; } = logs;

    /// <summary>
    /// Gets a value indicating whether this instance is executing.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is executing; otherwise, <c>false</c>.
    /// </value>
    public bool IsExecuting { get; private set; }
    /// <summary>
    /// Gets a value indicating whether [XML is provided].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [XML is provided]; otherwise, <c>false</c>.
    /// </value>
    public bool XmlIsProvided => xmlSplitter.XmlIsProvided;
    /// <summary>
    /// Gets a value indicating whether [uow is provided].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [uow is provided]; otherwise, <c>false</c>.
    /// </value>
    public bool UowIsProvided => xmlSplitter.UowIsProvided;
    /// <summary>
    /// Gets a value indicating whether [out dir is provided].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [out dir is provided]; otherwise, <c>false</c>.
    /// </value>
    public bool OutDirIsProvided => xmlSplitter.OutDirIsProvided;
    /// <summary>
    /// Gets a value indicating whether [program is provided].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [program is provided]; otherwise, <c>false</c>.
    /// </value>
    public bool ProgramIsProvided => xmlSplitter.ProgramIsProvided;
    /// <summary>
    /// Gets the possible states in manual.
    /// </summary>
    /// <value>
    /// The possible states in manual.
    /// </value>
    public IEnumerable<UowState> PossibleStatesInManual => xmlSplitter.PossibleStatesInManual.Values.Cast<UowState>();

    /// <summary>
    /// Gets or sets a value indicating whether this instance is all checked units-of-work states.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is all checked units-of-work states; otherwise, <c>false</c>.
    /// </value>
    public bool IsAllCheckedUowStates
    {
        get => PossibleStatesInManual.All(state => state.IsSelected = true);
        set => Array.ForEach(PossibleStatesInManual.ToArray(), state => state.IsSelected = value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is loading.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is loading; otherwise, <c>false</c>.
    /// </value>
    public bool IsLoading { get; set; }
    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    /// <value>
    /// The progress.
    /// </value>
    public double Progress { get; set; }

    /// <summary>
    /// Gets the program picker title.
    /// </summary>
    /// <value>
    /// The program picker title.
    /// </value>
    public string ProgramPickerTitle =>
        xmlSplitter.PossiblePrograms.Length > 0 ? "Select the" : string.Empty;

    /// <summary>
    /// Gets or sets the source XML.
    /// </summary>
    /// <value>
    /// The source XML.
    /// </value>
    public string SourceXml
    {
        get => xmlSplitter.XmlSourceFile;
        set
        {
            xmlSplitter.XmlSourceFile = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the units-of-work states file.
    /// </summary>
    /// <value>
    /// The units-of-work states file.
    /// </value>
    public string UowStatesFile
    {
        get => xmlSplitter.UowStatesFile;
        set
        {
            xmlSplitter.UowStatesFile = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the output directory.
    /// </summary>
    /// <value>
    /// The output directory.
    /// </value>
    public string OutputDirectory
    {
        get => xmlSplitter.OutputDirectory;
        set
        {
            xmlSplitter.OutputDirectory = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the program.
    /// </summary>
    /// <value>
    /// The program.
    /// </value>
    public string Program
    {
        get => xmlSplitter.Program;
        set
        {
            xmlSplitter.Program = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is ready to execute split.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is ready to execute split; otherwise, <c>false</c>.
    /// </value>
    public bool IsReadyToExecuteSplit => xmlSplitter.ExecuteSplitIsReady;
    /// <summary>
    /// Gets the x path.
    /// </summary>
    /// <value>
    /// The x path.
    /// </value>
    public string XPath => xmlSplitter.XPath;

    /// <summary>
    /// Gets the possible programs.
    /// </summary>
    /// <value>
    /// The possible programs.
    /// </value>
    public IEnumerable<string> PossiblePrograms => xmlSplitter.PossiblePrograms;

    /// <summary>
    /// Gets the activity.
    /// </summary>
    /// <value>
    /// The activity.
    /// </value>
    public string Activity => Logs.OrderBy(log => log.Key).LastOrDefault().Value.Message;

    /// <summary>
    /// Gets the status.
    /// </summary>
    /// <value>
    /// The status.
    /// </value>
    public (ProgressType Type, ProgressColor Color) Status
    {
        get
        {
            var type = Progress < 100.0 ? ProgressType.StripedAndAnimated : ProgressType.Default;
            return Logs.OrderBy(log => log.Key).LastOrDefault().Value.LogLevel switch
            {
                LogLevel.Warning => (type, ProgressColor.Warning),
                LogLevel.Critical or LogLevel.Error => (type, ProgressColor.Danger),
                LogLevel.Trace => (type, ProgressColor.Info),
                _ => (type, ProgressColor.Success)
            };
        }
    }

    /// <summary>
    /// Grids the data units-of-work states provider asynchronous.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    public async Task<GridDataProviderResult<UowState>> GridDataUowStatesProviderAsync(
        GridDataProviderRequest<UowState> request)
    {
        return await Task.FromResult(request.ApplyTo(PossibleStatesInManual));
    }


    /// <summary>
    /// Picks the XML file.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    public async Task PickXmlFile(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(Timeout);
        PickOptions options = new()
        {
            FileTypes = XmlType,
            PickerTitle = "Select XML File"
        };
        try
        {
            if (await FilePicker.Default.PickAsync(options) is { } result)
            {
                SourceXml = result.FullPath;
                LoadingXmlFile = true;
                Dispatcher.Dispatch(new PickFileAction(SourceXml));
                xmlSplitter.XmlContent = await File.ReadAllTextAsync(SourceXml, cts.Token).ConfigureAwait(false);

            }
        }
        catch (OperationCanceledException)
        {
            var option = new ModalOption()
            {
                FooterButtonColor = ButtonColor.Warning,
                FooterButtonText = "OK",
                IsVerticallyCentered = true,
                Message = "The XML loading was cancelled",
                ShowFooterButton = true,
                Size = ModalSize.Regular,
                Title = "Cancelled XML Selection",
                Type = ModalType.Warning
            };
            await ModalService.ShowAsync(option);
        }
        finally
        {
            LoadingXmlFile = false;
        }
    }


    /// <summary>
    /// Picks the units-of-work states file.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    public async Task PickUowStatesFile(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(Timeout);
        PickOptions options = new()
        {
            FileTypes = UowType,
            PickerTitle = "Select UOW File"
        };
        try
        {
            if (await FilePicker.Default.PickAsync(options) is { } result)
            {
                UowStatesFile = result.FullPath;
                LoadingUowStatesFile = true;
                UowLoadSuccess = await xmlSplitter.UowPreliminaryCheck(UowStatesFile).ConfigureAwait(false);
                if (!UowLoadSuccess.Success) return;
                xmlSplitter.UowContent = await File.ReadAllTextAsync(UowStatesFile, cts.Token).ConfigureAwait(false);
                xmlSplitter.StateMatches = await xmlSplitter.TryGetUowMatchesAsync(cts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // TODO: Show message to user
            Debug.WriteLine($"{nameof(PickUowStatesFile)} was cancelled");
        }
        finally
        {
            LoadingUowStatesFile = false;
        }
    }

    public async Task PickOutputFolder(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(Timeout);
        if (await FolderPicker.Default.PickAsync(cts.Token) is { Folder: { } folder }) OutputDirectory = folder.Path;
    }

    public async Task SplitXmlCommand(object sender, EventArgs e, CancellationToken token = default)
    {
        IsExecuting = true;
        // TODO: Compare docnbr to UowStatesDocnbr
        await xmlSplitter.ExecuteSplit(sender, e, new Progress<double>(value => Progress = value), token)
            .ConfigureAwait(true);
        IsExecuting = false;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        var cts = new CancellationTokenSource();
        try
        {
            logger.LogInformation("Initializing XmlSplitterViewModel");
            await xmlSplitter.LoadAssets(cts.Token);
            logger.LogInformation("Initialized XmlSplitterViewModel");
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Initialization was cancelled");
        }
        catch when (!Debugger.IsAttached)
        {
            //TODO: Show message to user
        }

        IsLoading = false;
    }

    public void ClearLogs(ItemClickEventArgs e)
    {
        Debug.WriteLine(
            $"Item Clicked => Menu: {e.ContextMenuId}, MenuTarget: {e.ContextMenuTargetId}, IsCanceled: {e.IsCanceled}, MenuItem: {e.MenuItemElement}, MouseEvent: {e.MouseEvent}");
        Logs.Clear();
    }

    public void SelectStates()
    {
        // return States is null ? [] : [.. States.Where(s => s.IsSelected)];
    }


    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        SelectUowModal.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await SelectUowModal.DisposeAsync();
    }
}