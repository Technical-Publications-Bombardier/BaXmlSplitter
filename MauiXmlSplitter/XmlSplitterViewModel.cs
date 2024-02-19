using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using BlazorBootstrap;
using BlazorContextMenu;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiXmlSplitter.Models;
using MauiXmlSplitter.Resources;
using MauiXmlSplitter.Shared;
using Microsoft.Extensions.Logging;

namespace MauiXmlSplitter;

/// <summary>
///     View model for XML Splitter.
/// </summary>
/// <seealso cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject" />
public partial class XmlSplitterViewModel(
    CultureInfo locale,
    ConcurrentDictionary<DateTime, LogRecord> logs,
    ILogger<XmlSplitterViewModel> logger,
    ModalService modalService,
    XmlSplitReport report
)
    : ObservableObject, IDisposable, IAsyncDisposable

{
    /// <summary>
    ///     The units-of-work failure reason elaboration
    /// </summary>
    public static readonly Dictionary<XmlSplitter.ReasonForUowFailure, string> UowFailureReasonElaboration = new()
    {
        { XmlSplitter.ReasonForUowFailure.None, string.Empty },
        { XmlSplitter.ReasonForUowFailure.NoFileProvided, AppResources.NoFileProvided },
        { XmlSplitter.ReasonForUowFailure.NotPlaintextFile, AppResources.FileAppearsNotToBePlaintext },
        { XmlSplitter.ReasonForUowFailure.NotEnoughContent, AppResources.FileWasEmptyOrTooShort },
        { XmlSplitter.ReasonForUowFailure.DidNotMeetExpectedPattern, AppResources.FileDidNotMeetExpectedPattern }
    };

    /// <summary>
    ///     The XML file type
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
    ///     The units-of-work states file type (plaintext)
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
    ///     The timeout
    /// </summary>
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5.0);


    /// <summary>
    ///     The XML splitter
    /// </summary>
    private readonly XmlSplitter xmlSplitter = new(logger, modalService, report);

    /// <summary>
    ///     The invalid units-of-work states file modal
    /// </summary>
    public Modal InvalidUowStatesFileModal = default!;

    /// <summary>
    ///     The loading units-of-work states file
    /// </summary>
    [ObservableProperty] private bool loadingUowStatesFile;

    /// <summary>
    ///     The loading XML file
    /// </summary>
    [ObservableProperty] private bool loadingXmlFile;

    /// <summary>
    ///     The modal service
    /// </summary>
    public ModalService ModalService = modalService;


    /// <summary>
    ///     The select units-of-work modal
    /// </summary>
    public Modal SelectUowModal = default!;

    /// <summary>
    ///     The states
    /// </summary>
    [ObservableProperty] private IQueryable<UowState>? states;

    /// <summary>
    ///     Whether the states are selected
    /// </summary>
    [ObservableProperty] private bool statesAreSelected;


    /// <summary>
    ///     The units-of-work load success
    /// </summary>
    [ObservableProperty] private (bool Success, XmlSplitter.ReasonForUowFailure Reason) uowLoadSuccess = (false,
        XmlSplitter.ReasonForUowFailure.NoFileProvided);

    /// <summary>
    ///     Gets or sets the context menu style.
    /// </summary>
    /// <value>
    ///     The context menu style.
    /// </value>
    public string? ContextMenuStyle { get; set; }

    /// <summary>
    ///     Gets the logs.
    /// </summary>
    /// <value>
    ///     The logs.
    /// </value>
    public ConcurrentDictionary<DateTime, LogRecord> Logs { get; } = logs;

    /// <summary>
    ///     Gets a value indicating whether this instance is executing.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is executing; otherwise, <c>false</c>.
    /// </value>
    public bool IsExecuting { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether [XML is provided].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [XML is provided]; otherwise, <c>false</c>.
    /// </value>
    public bool XmlIsProvided => xmlSplitter.XmlIsProvided;

    /// <summary>
    ///     Gets a value indicating whether [uow is provided].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [uow is provided]; otherwise, <c>false</c>.
    /// </value>
    public bool UowIsProvided => xmlSplitter.UowIsProvided;

    /// <summary>
    ///     Gets a value indicating whether [out dir is provided].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [out dir is provided]; otherwise, <c>false</c>.
    /// </value>
    public bool OutDirIsProvided => xmlSplitter.OutDirIsProvided;

    /// <summary>
    ///     Gets a value indicating whether [program is provided].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [program is provided]; otherwise, <c>false</c>.
    /// </value>
    public bool ProgramIsProvided => xmlSplitter.ProgramIsProvided;

    /// <summary>
    ///     Gets the possible states in manual.
    /// </summary>
    /// <value>
    ///     The possible states in manual.
    /// </value>
    public IEnumerable<UowState> PossibleStatesInManual => xmlSplitter.PossibleStatesInManual.Values.Cast<UowState>();

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is all checked units-of-work states.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is all checked units-of-work states; otherwise, <c>false</c>.
    /// </value>
    public bool IsAllCheckedUowStates
    {
        get => PossibleStatesInManual.All(state => state.IsSelected = true);
        set => Array.ForEach(PossibleStatesInManual.ToArray(), state => state.IsSelected = value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is loading.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is loading; otherwise, <c>false</c>.
    /// </value>
    public bool IsLoading { get; set; }

    /// <summary>
    ///     Gets or sets the progress.
    /// </summary>
    /// <value>
    ///     The progress.
    /// </value>
    public double Progress { get; set; }

    /// <summary>
    ///     Gets the program picker title.
    /// </summary>
    /// <value>
    ///     The program picker title.
    /// </value>
    public string ProgramPickerTitle =>
        xmlSplitter.PossiblePrograms.Length > 0 ? "Select the" : string.Empty;

    /// <summary>
    ///     Gets or sets the source XML.
    /// </summary>
    /// <value>
    ///     The source XML.
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

    public TaskCompletionSource<bool> UowModalClosed { get; set; } = new();


    /// <summary>
    ///     Gets or sets the units-of-work states file.
    /// </summary>
    /// <value>
    ///     The units-of-work states file.
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
    ///     Gets or sets the output directory.
    /// </summary>
    /// <value>
    ///     The output directory.
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
    ///     Gets or sets the program.
    /// </summary>
    /// <value>
    ///     The program.
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
    ///     Gets a value indicating whether this instance is ready to execute split.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is ready to execute split; otherwise, <c>false</c>.
    /// </value>
    public bool IsReadyToExecuteSplit => xmlSplitter.ExecuteSplitIsReady;

    /// <summary>
    ///     Gets the x path.
    /// </summary>
    /// <value>
    ///     The x path.
    /// </value>
    public string XPath => xmlSplitter.XPath;

    /// <summary>
    ///     Gets the possible programs.
    /// </summary>
    /// <value>
    ///     The possible programs.
    /// </value>
    public IEnumerable<string> PossiblePrograms => xmlSplitter.PossiblePrograms;

    /// <summary>
    ///     Gets the activity.
    /// </summary>
    /// <value>
    ///     The activity.
    /// </value>
    public string Activity => Logs.OrderBy(log => log.Key).LastOrDefault().Value.Message;

    /// <summary>
    ///     Gets the status.
    /// </summary>
    /// <value>
    ///     The status.
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

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await SelectUowModal.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        SelectUowModal.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Provides the units-of-work states asynchronously for the <see cref="BlazorBootstrap.Grid{UowState}" />.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    public async Task<GridDataProviderResult<UowState>> GridDataUowStatesProviderAsync(
        GridDataProviderRequest<UowState> request)
    {
        return await Task.FromResult(request.ApplyTo(PossibleStatesInManual));
    }

    /// <summary>
    /// Called when [selected uow states changed].
    /// </summary>
    /// <param name="chosenStates">The chosen states.</param>
    /// <returns></returns>
    public async Task OnSelectedUowStatesChanged(HashSet<UowState> chosenStates)
    {
        States = chosenStates.AsQueryable();
        await Task.CompletedTask;
    }

    /// <summary>
    ///     Picks the XML file.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    public async Task PickXmlFile(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(Timeout);
        PickOptions options = new()
        {
            FileTypes = XmlType,
            PickerTitle = AppResources.SelectXMLFile
        };
        if (await FilePicker.Default.PickAsync(options) is { FullPath: not null } result)
            await SelectXmlFile(result.FullPath, cts.Token);
    }

    /// <summary>
    /// Helper method for text field (by path) or GUI XML file selection.
    /// </summary>
    /// <param name="fullPath">The full path to the XML file.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    private async Task SelectXmlFile(string fullPath, CancellationToken token)
    {
        try
        {
            if (SourceXml != fullPath)
                SourceXml = fullPath;
            LoadingXmlFile = true;
            xmlSplitter.XmlContent = await File.ReadAllTextAsync(SourceXml, token).ConfigureAwait(false);
            xmlSplitter.XmlSourceFile = SourceXml;
        }
        catch (OperationCanceledException)
        {
            var option = new ModalOption
            {
                FooterButtonColor = ButtonColor.Warning,
                FooterButtonText = AppResources.OK,
                IsVerticallyCentered = true,
                Message = AppResources.TheXMLLoadingWasCancelled,
                ShowFooterButton = true,
                Size = ModalSize.Regular,
                Title = AppResources.CancelledXMLSelection,
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
    ///     Picks the units-of-work states file.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    public async Task PickUowStatesFile(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(Timeout);
        PickOptions options = new()
        {
            FileTypes = UowType,
            PickerTitle = AppResources.SelectUOWFile
        };
        if (await FilePicker.Default.PickAsync(options) is { FullPath: not null } result)
            await SelectUowFile(result.FullPath, cts.Token);
    }

    private async Task SelectUowFile(string fullPath, CancellationToken token)
    {
        try
        {
            if (UowStatesFile != fullPath)
                UowStatesFile = fullPath;
            LoadingUowStatesFile = true;
            UowLoadSuccess = await xmlSplitter.UowPreliminaryCheck(UowStatesFile).ConfigureAwait(false);
            if (!UowLoadSuccess.Success) return;
            xmlSplitter.UowContent = await File.ReadAllTextAsync(UowStatesFile, token).ConfigureAwait(false);
            xmlSplitter.StateMatches = await xmlSplitter.TryGetUowMatchesAsync(token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine($"{nameof(PickUowStatesFile)} was cancelled");
            logger.LogDebug($"{nameof(PickUowStatesFile)} was cancelled");
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
        try
        {
            var docnbrFromXml = DocnbrFromXmlRe().Match(xmlSplitter.XmlContent).Groups["docnbr"].Value;
            var docnbrFromUow = DocnbrFromUowRe().Match(xmlSplitter.UowContent).Groups["docnbr"].Value;

            if (string.IsNullOrEmpty(docnbrFromXml))
                logger.LogCritical(AppResources.CouldNotReadDocnbrFromXML);
            if (string.IsNullOrEmpty(docnbrFromUow))
                logger.LogCritical(AppResources.CouldNotReadDocnbrFromUOWStatesFile);
            if (docnbrFromUow != docnbrFromXml)
                logger.LogCritical(AppResources.XMLDocnbrXmlDocnbrDoesNotMatchUOWStatesFileDocnbrUowDocnbr,
                    docnbrFromXml, docnbrFromUow);
            xmlSplitter.UowStates = await xmlSplitter.ParseUowContentAsync(token).ConfigureAwait(false);
            if (xmlSplitter.UowStates is not { } parsedStates)
            {
                logger.LogCritical("Could not parse UOW states file");
                return;
            }
            UowModalClosed = new TaskCompletionSource<bool>();
            await SelectUowModal.ShowAsync().ConfigureAwait(true); // Sets this.States
            await UowModalClosed.Task.ConfigureAwait(true); // Wait for the modal to close
            if (States is null || !StatesAreSelected)
            {
                logger.LogWarning("No UOW states selected");
                return;
            }
            xmlSplitter.FullyQualifiedSelectedStates = parsedStates.Where(state =>
                States.Select(uow => uow.StateValue).Contains(state.StateValue));
            await xmlSplitter.ExecuteSplit(sender, e, new Progress<double>(value => Progress = value), token)
                .ConfigureAwait(true);
        }
        finally
        {
            IsExecuting = false;
        }
    }

    /// <summary>
    ///     Initializes the Xml Splitter View Model asynchronously.
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        IsLoading = true;
        var cts = new CancellationTokenSource();
        try
        {
            logger.LogInformation(AppResources.InitializingXmlSplitterViewModel);
            PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName != nameof(SourceXml) ||
                    Path.GetDirectoryName(SourceXml) is not { } xmlDirectory) return;
                await SelectXmlFile(SourceXml, cts.Token);
                var wipDir = new char[xmlDirectory.Length + 1 + XmlSplitter.DefaultOutputDir.Length];
                OutputDirectory = Path.TryJoin(Path.GetDirectoryName(SourceXml), XmlSplitter.DefaultOutputDir,
                    wipDir, out _)
                    ? string.Concat(wipDir)
                    : OutputDirectory;
            };
            PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == nameof(UowStatesFile)) await SelectUowFile(UowStatesFile, cts.Token);
            };
            await xmlSplitter.LoadAssets(cts.Token);
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = locale;
            logger.LogInformation(AppResources.InitializedXmlSplitterViewModel);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine(AppResources.InitializationWasCancelled);
        }
        catch when (!Debugger.IsAttached)
        {
            ModalOption option = new()
            {
                FooterButtonColor = ButtonColor.Danger,
                FooterButtonText = AppResources.Reload,
                IsVerticallyCentered = true,
                Message = AppResources.CouldNotLoadStaticResources,
                ShowFooterButton = true,
                Size = ModalSize.Regular,
                Title = AppResources.ErrorInitializing,
                Type = ModalType.Danger
            };
            await ModalService.ShowAsync(option).ContinueWith(_ => InitializeAsync(), cts.Token);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public string XmlSourceFileBaseName => xmlSplitter.XmlSourceFileBaseName;
    /// <summary>
    ///     Clears the logs.
    /// </summary>
    /// <param name="e">The <see cref="BlazorContextMenu.ItemClickEventArgs" /> instance containing the event data.</param>
    public void ClearLogs(ItemClickEventArgs e)
    {
        Debug.WriteLine(
            $"Item Clicked => Menu: {e.ContextMenuId}, MenuTarget: {e.ContextMenuTargetId}, IsCanceled: {e.IsCanceled}, MenuItem: {e.MenuItemElement}, MouseEvent: {e.MouseEvent}");
        Logs.Clear();
    }

    [GeneratedRegex("""docnbr="(?<docnbr>[^"]+)""")]
    private static partial Regex DocnbrFromXmlRe();

    [GeneratedRegex(@"^(?<docnbr>[^\r\n]*)")]
    private static partial Regex DocnbrFromUowRe();
}