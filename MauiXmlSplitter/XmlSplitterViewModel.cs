using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Storage;
using MauiXmlSplitter.Models;
using Microsoft.Extensions.Logging;

namespace MauiXmlSplitter;

public class XmlSplitterViewModel(ILogger<XmlSplitterViewModel> logger) : INotifyPropertyChanged
{
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

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2.0);


    private readonly ILogger<XmlSplitterViewModel> logger = logger;

    private readonly XmlSplitter xmlSplitter = new(logger);

    public bool IsExecuting { get; set; }
    public bool XmlIsProvided => xmlSplitter.XmlIsProvided;
    public bool UowIsProvided => xmlSplitter.UowIsProvided;
    public bool OutDirIsProvided => xmlSplitter.OutDirIsProvided;
    public bool ProgramIsProvided => xmlSplitter.ProgramIsProvided;
    public bool IsLoading { get; set; }
    public double Progress { get; set; }

    public string ProgramPickerTitle =>
        xmlSplitter.PossiblePrograms.Length > 0 ? "Select the CSDB Program" : "CSDB Program";

    public string SourceXml
    {
        get => xmlSplitter.XmlSourceFile;
        set
        {
            xmlSplitter.XmlSourceFile = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceXml)));
        }
    }

    public string UowStatesFile
    {
        get => xmlSplitter.UowStatesFile;
        set
        {
            xmlSplitter.UowStatesFile = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UowStatesFile)));
        }
    }

    public string OutputDirectory
    {
        get => xmlSplitter.OutputDirectory;
        set
        {
            xmlSplitter.OutputDirectory = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutputDirectory)));
        }
    }

    public bool IsReadyToExecuteSplit => xmlSplitter.ExecuteSplitIsReady;
    public string XPath => xmlSplitter.XPath;

    public IEnumerable<string> PossiblePrograms => xmlSplitter.PossiblePrograms;

    public event PropertyChangedEventHandler? PropertyChanged;

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
            if (await FilePicker.Default.PickAsync(options) is { } result) SourceXml = result.FullPath;
            xmlSplitter.XmlContent = await File.ReadAllTextAsync(SourceXml, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

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
            if (await FilePicker.Default.PickAsync(options) is { } result) UowStatesFile = result.FullPath;
            xmlSplitter.UowContent = await File.ReadAllTextAsync(UowStatesFile, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!Debugger.IsAttached)
        {
        }
    }

    public async Task PickOutputFolder(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(Timeout);
        if (await FolderPicker.Default.PickAsync(cts.Token) is { Folder: { } folder }) OutputDirectory = folder.Path;
    }

    public async Task SplitXmlCommand(object sender, EventArgs e)
    {
        IsExecuting = true;
        // TODO: Compare docnbr to UowStatesDocnbr
        await xmlSplitter.ExecuteSplit(sender, e, new Progress<double>(value => Progress = value)).ConfigureAwait(true);
        IsExecuting = false;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        var cts = new CancellationTokenSource();
        try
        {
            await xmlSplitter.LoadAssets(cts.Token);
        }
        catch (OperationCanceledException) when (!Debugger.IsAttached)
        {
            throw;
        }
        catch when (!Debugger.IsAttached)
        {
            throw;
        }

        IsLoading = false;
    }
}