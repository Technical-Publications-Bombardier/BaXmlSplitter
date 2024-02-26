using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using BlazorBootstrap;
using CommunityToolkit.Mvvm.ComponentModel;
using F23.StringSimilarity;
using MauiXmlSplitter.Pages;
using MauiXmlSplitter.Resources;
using Microsoft.Extensions.Logging;
using static MauiXmlSplitter.Models.CsdbContext;
using Path = System.IO.Path;

namespace MauiXmlSplitter.Models;

/// <summary>
///     XML Splitter class
/// </summary>
public partial class XmlSplitter(ILogger logger, ModalService modalService, IXmlSplitReport<XmlSplitter> report, BaXmlDocument xml) : ObservableObject
{
    public enum ReasonForUowFailure
    {
        None,
        NoFileProvided,
        NotPlaintextFile,
        NotEnoughContent,
        DidNotMeetExpectedPattern
    }

    /// <summary>
    ///     The default output directory
    /// </summary>
    public const string DefaultOutputDir = "WIP";

    /// <summary>The CSDB programs.</summary>
    internal static readonly string[] Programs =
        Enum.GetNames<CsdbProgram>().Where(e => e != Enum.GetName(CsdbProgram.None)).ToArray();

    /// <summary>
    ///     The <see cref="F23.StringSimilarity.Jaccard" /> object for performing string similarity calculations.
    /// </summary>
    internal static readonly Jaccard Jaccard = new(2);

    private readonly JsonSerializerOptions options = new();


    /// <summary>
    ///     The CSDB element names eligible for importing to RWS Contenta. Initialized by
    ///     <see cref="LoadAssets(CancellationToken)" />
    /// </summary>
    private Dictionary<CsdbProgram, Dictionary<string, string[]>>? checkoutItems;

    /// <summary>
    ///     The CSDB program (GXPROD, CTALPROD, B_IFM, CH604PROD, LJ4045PROD)
    ///     for the manual.
    /// </summary>
    /// <seealso cref="Program" />
    private CsdbProgram csdbProgram;

    private IEnumerable<CharacterEntities>? lookupEntities;

    /// <summary>The lookup table for manual type from docnbr.</summary>
    private Dictionary<CsdbProgram, Dictionary<string, string>>? manualFromDocnbr;

    /// <summary>
    ///     The directory to which the split XML files will be written. <see cref="DefaultOutputDir">By default</see>, this
    ///     will be the <c>"WIP"</c> directory in the same directory as the source XML file.
    /// </summary>
    private string? outputDirectory;

    private Hashtable? possibleStatesInManual;


    /// <summary>
    ///     The cancellation token for the previous match work in the <see cref="TryGetUowMatchesAsync(CancellationToken)" />.
    ///     Defaults to 45 seconds.
    /// </summary>
    private CancellationTokenSource previousUowCts = new(TimeSpan.FromSeconds(45));

    /// <summary>
    ///     The <see cref="CsdbProgram" /> lookup from docnbr
    /// </summary>
    private Dictionary<string, CsdbProgram[]>? programPerDocnbr;

    /// <summary>A dictionary of all the states in any CSDB program (GXPROD, CTALPROD, B_IFM, CH604PROD, LJ4045PROD).</summary>
    private Dictionary<CsdbProgram, Dictionary<int, UowState>>? statesPerProgram;

    /// <summary>
    ///     The content of the UOW states file. This is loaded into memory as soon as the UOW states file is selected.
    /// </summary>
    private string? uowContent;

    /// <summary>The path to the file that contains the work states for the units of work (UOW) in the XML.</summary>
    private string? uowStatesFile;

    /// <summary>
    ///     The string content of the XML file on which the split will be performed. This is
    ///     loaded into memory as soon as the XML is selected.
    /// </summary>
    private string? xmlContent;

    /// <summary>
    ///     The path to the source XML file on which the split will be performed.
    /// </summary>
    private string? xmlSourceFile;


    /// <summary>The fully populated unit-of-work states that are selected by the user for export.</summary>
    public IEnumerable<UowState>? FullyQualifiedSelectedStates { get; set; }

    /// <summary>
    ///     Gets or sets the UOW states as defined in the UOW states file.
    /// </summary>
    /// <value>
    ///     The uow states.
    /// </value>
    public IEnumerable<UowState>? UowStates { get; set; }

    public Hashtable PossibleStatesInManual => possibleStatesInManual ?? [];

    public string UowContent
    {
        get => uowContent ?? string.Empty;
        set => uowContent = value;
    }

    private byte[] xmlContentHash = [];

    public string XmlContent
    {
        get => xmlContent ?? string.Empty;
        set
        {
            xmlContent = value;
            OnPropertyChanged();
            if (string.IsNullOrEmpty(xmlContent)) return;
            var newContentHash = MD5.HashData(Encoding.UTF8.GetBytes(xmlContent));
            if (xmlContentHash == newContentHash)
                return;
            xmlContentHash = newContentHash;
            try
            {
                xml = new BaXmlDocument();
                xml.LoadXml(xmlContent);
            }
            catch (XmlException exception)
            {
                logger.LogError(exception, AppResources.FailedToLoadXMLMessage, exception.Message);
            }
        }
    }

    /// <summary>
    ///     The XPath string that would be used to select the nodes to split from the XML. This is calculated from the tag
    ///     names and key values in the UOW states file.
    /// </summary>
    /// <value>
    ///     A join on the XPath for all states in the <see cref="FullyQualifiedSelectedStates" />.
    /// </value>
    public string XPath => FullyQualifiedSelectedStates is null
        ? string.Empty
        : string.Join('|', FullyQualifiedSelectedStates.Select(state => state.XPath));

    /// <summary>
    ///     Gets the docnbr.
    /// </summary>
    /// <value>
    ///     The docnbr.
    /// </value>
    public string Docnbr
    {
        get
        {
            if (xml.DocumentElement is not { } docElement || !docElement.HasAttribute("docnbr")) return string.Empty;
            return docElement.GetAttribute("docnbr");
        }
    }

    /// <summary>
    ///     Gets or sets the source XML file path.
    /// </summary>
    /// <value>
    ///     The source XML file path.
    /// </value>
    public string XmlSourceFile
    {
        get => xmlSourceFile ?? string.Empty;
        set => xmlSourceFile = value;
    }

    /// <summary>
    ///     Gets the name of the XML source file base.
    /// </summary>
    /// <value>
    ///     The name of the XML source file base.
    /// </value>
    public string XmlSourceFileBaseName => XmlFilenameRe()
        .Replace(Path.GetFileNameWithoutExtension(xmlSourceFile ?? string.Empty),
            m => TerminusCharPattern().Replace(m.Groups["basename"].Value, string.Empty));

    /// <summary>
    ///     Gets or sets path to the file that contains the work states for the units of work (UOW) in the XML.
    /// </summary>
    /// <value>
    ///     The path to the file that contains the work states for the units of work (UOW) in the XML.
    /// </value>
    public string UowStatesFile
    {
        get => uowStatesFile ?? string.Empty;
        set => uowStatesFile = value;
    }

    /// <summary>
    ///     Gets or sets the output directory path.
    /// </summary>
    /// <value>
    ///     The output directory path.
    /// </value>
    public string OutputDirectory
    {
        get => outputDirectory ?? string.Empty;
        set => outputDirectory = value;
    }

    /// <summary>
    ///     Gets the possible <see cref="CsdbProgram" /> programs.
    /// </summary>
    /// <value>
    ///     The possible <see cref="CsdbProgram" /> programs.
    /// </value>
    public string[] PossiblePrograms
    {
        get
        {
            if (string.IsNullOrEmpty(Docnbr) || programPerDocnbr == null)
                return Programs;
            return [.. programPerDocnbr[Docnbr].Select(Enum.GetName)];
        }
    }

    /// <summary>
    ///     Gets or sets the CSDB program (GXPROD, CTALPROD, B_IFM, CH604PROD, LJ4045PROD).
    /// </summary>
    /// <value>
    ///     The CSDB program (GXPROD, CTALPROD, B_IFM, CH604PROD, LJ4045PROD).
    /// </value>
    /// <seealso cref="csdbProgram" />
    public string Program
    {
        get => (csdbProgram == CsdbProgram.None ? string.Empty : Enum.GetName(csdbProgram)) ?? string.Empty;
        set
        {
            if (!Enum.TryParse(value, true, out csdbProgram)) csdbProgram = CsdbProgram.None;
        }
    }

    private bool IsLoadingXml { get; }

    public string Manual
    {
        get
        {
            if (string.IsNullOrEmpty(Program) || string.IsNullOrEmpty(Docnbr) || manualFromDocnbr == null)
                return string.Empty;
            return manualFromDocnbr[csdbProgram][Docnbr];
        }
    }

    /// <summary>
    ///     The XML is selected.
    /// </summary>
    /// <returns>
    ///     Returns <c>true</c> if <see cref="xmlSourceFile" /> and <see cref="xmlContent" /> are both not null or empty,
    ///     <c>false</c> otherwise.
    /// </returns>
    public bool XmlIsProvided =>
        !IsLoadingXml && !string.IsNullOrEmpty(xmlSourceFile) && !string.IsNullOrEmpty(xmlContent);

    /// <summary>
    ///     Units of work states for export are selected.
    /// </summary>
    /// <returns>
    ///     Returns <c>true</c> if <see cref="uowStatesFile" /> and <see cref="uowContent" /> are both not null or empty,
    ///     <c>false</c> otherwise.
    /// </returns>
    public bool UowIsProvided => !string.IsNullOrEmpty(uowStatesFile) && !string.IsNullOrEmpty(uowContent);

    /// <summary>
    ///     Output directory is selected.
    /// </summary>
    /// <returns>Returns <c>true</c> if <see cref="outputDirectory" /> is not null or empty, <c>false</c> otherwise.</returns>
    public bool OutDirIsProvided => !string.IsNullOrEmpty(outputDirectory);

    /// <summary>
    ///     The program for the manual is selected.
    /// </summary>
    /// <returns>Returns <c>true</c> if <see cref="Program" /> is not null or empty, <c>false</c> otherwise.</returns>
    public bool ProgramIsProvided => !string.IsNullOrEmpty(Program);

    /// <summary>
    ///     Gets a value indicating whether all criteria are met to begin the splitting.
    /// </summary>
    /// <value>
    ///     <c>true</c> if all criteria are met to begin the splitting; otherwise, <c>false</c>.
    /// </value>
    public bool ExecuteSplitIsReady => XmlIsProvided && UowIsProvided && OutDirIsProvided && ProgramIsProvided;

    public string UowStatesDocnbr { get; private set; }

    internal MatchCollection? StateMatches { get; set; }

    internal async Task<MatchCollection?> TryGetUowMatchesAsync(CancellationToken token)
    {
        await previousUowCts.CancelAsync().ConfigureAwait(false); // Supplant previous run
        previousUowCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        try
        {
            return await Task.Run(() => UowRegex().Matches(UowContent), previousUowCts.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    internal async Task<(bool, ReasonForUowFailure)> UowPreliminaryCheck(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return (false, ReasonForUowFailure.NoFileProvided);
        const int numLines = 5; // number of lines to check provisionally
        var isUowText =
            (Success: true, Reason: ReasonForUowFailure.None); // assume file is text until assumption is falsified
        await Task.Run(CheckLinesArePlaintext);
        return isUowText;

        bool CheckLineIsPlaintext(in string line)
        {
            var isPlaintext = true;
            for (var j = 0; j < line.Length && isPlaintext; j++)
                isPlaintext =
                    line[j] == '\t' || !char.IsControl(line[j]); // Control characters indicate file is not plaintext
            return isPlaintext;
        }

        void CheckLinesArePlaintext()
        {
            using var sr = new StreamReader(filePath);
            var provisionalUowStatesDocnbr = UowStatesDocnbr;
            int i;
            for (i = 0; i < numLines && isUowText.Success; i++)
            {
                if (sr.EndOfStream || sr.ReadLine() is not { } line) break;
                if (i > 0)
                {
                    isUowText.Success = UowRegex().IsMatch(line);
                    if (!isUowText.Success) isUowText.Reason = ReasonForUowFailure.DidNotMeetExpectedPattern;
                }
                else
                {
                    provisionalUowStatesDocnbr =
                        line; // Use this opportunity to set the expected docnbr from the UOW states file
                }

                if (!CheckLineIsPlaintext(line))
                    isUowText = (Success: false, Reason: ReasonForUowFailure.NotPlaintextFile);
            }

            isUowText.Success &= i > 1;
            if (i <= 1) isUowText.Reason = ReasonForUowFailure.NotEnoughContent;
            if (isUowText.Success) UowStatesDocnbr = provisionalUowStatesDocnbr;
        }
    }

    [GeneratedRegex(
        """(?<tabs>\t*)(?:Front Matter: )?(?<tag>\S+)(?: (?<key>\S+))?(?: (?<rs>RS-\d+))?(?: - (?<title>.+?))?(?: (?<lvl>[A-Z0-9 =]+?))? +-- .*?\(state = "(?<state>[^"]*)"\)$""",
        RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex UowRegex();

    private async Task<string[]> GetCheckoutElementNamesAsync()
    {
        if (string.IsNullOrEmpty(Manual) || string.IsNullOrEmpty(Program) || checkoutItems == null)
            return [];

        var closestManual = await Task.Run(() =>
        {
            var bestMatchManualKey = new PriorityQueue<string, double>();
            foreach (var manualNameKey in checkoutItems[csdbProgram].Keys)
                bestMatchManualKey.Enqueue(manualNameKey, Jaccard.Distance(Manual, manualNameKey));
            return bestMatchManualKey.Dequeue();
        });
        if(Manual != closestManual) logger.LogTrace(AppResources.ClosestManualFoundForProgramManualIsClosestManual, Program, Manual,
            closestManual);
        return checkoutItems[csdbProgram][closestManual];
    }

    /// <summary>
    ///     Asynchronously gets the XML nodes by checkout element names. The <see cref="XPath"/> also restricts the list to just the selected states.
    /// </summary>
    /// <returns></returns>
    private async Task<XmlNode[]> GetXmlNodesAsync()
    {
        if (!xml.HasChildNodes || string.IsNullOrEmpty(XPath))
            return [];
        var checkoutElementNames = await GetCheckoutElementNamesAsync().ConfigureAwait(false);
        if (checkoutElementNames.Length == 0)
        {
            if (await Task.Run(() => xml.SelectNodes(XPath)).ConfigureAwait(false) is { } plainNodes)
                return plainNodes.Cast<XmlNode>().ToArray();
            return [];
        }

        if (await Task.Run(() => xml.SelectNodesByCheckout(XPath, checkoutElementNames)).ConfigureAwait(false) is
            { } checkoutNodes)
            return checkoutNodes;
        return [];
    }

    [GeneratedRegex("[_-]$")]
    private static partial Regex TerminusCharPattern();

    /// <summary>The XML filename regular expression pattern.</summary>
    [GeneratedRegex(@"(?<basename>[\w_-]+[\d-]{8,}).*", RegexOptions.Compiled | RegexOptions.Multiline, 15*1000)]
    internal static partial Regex XmlFilenameRe();

    /// <summary>
    ///     Loads the static assets.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns></returns>
    public async Task LoadAssets(CancellationToken token = default)
    {
        try
        {
            // add the custom converters to the JsonSerializerOptions options

            foreach (var converterType in new[]
                     {
                         typeof(CsdbProgramConverter), typeof(UowStateConverter), typeof(CharacterEntitiesConverter)
                     })
                if (options.Converters.All(c => c.GetType() != converterType) &&
                    Activator.CreateInstance(converterType) as JsonConverter is { } converter)
                    options.Converters.Add(converter);

            if (checkoutItems is not { Count: > 0 })
            {
                checkoutItems = await DeserializeCheckoutItems(token);
                logger.LogTrace(AppResources.DeserializedCheckoutItemsWithNumItemsItems, checkoutItems.Count);
                Debug.Assert(checkoutItems is { Count: > 0 }, "Checkout items were empty");
            }

            if (manualFromDocnbr is not { Count: > 0 })
            {
                manualFromDocnbr = await DeserializeDocnbrManualFromProgram(token);
                logger.LogTrace(AppResources.DeserializedDocnbrManualFromProgramWithNumItemsItems,
                    manualFromDocnbr.Count);
                Debug.Assert(manualFromDocnbr is { Count: > 0 }, "Manual from docnbr was empty");
            }

            if (programPerDocnbr is not { Count: > 0 })
            {
                programPerDocnbr = await DeserializeProgramPerDocnbr(token);
                logger.LogTrace(AppResources.DeserializedProgramPerDocnbrWithNumItemsItems, programPerDocnbr.Count);
                Debug.Assert(programPerDocnbr is { Count: > 0 }, "Program per docnbr was empty");
            }

            if (statesPerProgram is not { Count: > 0 })
            {
                statesPerProgram = await DeserializeStatesPerProgram(token);
                logger.LogTrace(AppResources.DeserializedStatesPerProgramWithNumItemsItems, statesPerProgram.Count);
                Debug.Assert(statesPerProgram is { Count: > 0 }, "States per program was empty");
            }

            if (lookupEntities?.Any() != true)
            {
                lookupEntities = await DeserializeLookupEntities(token);
                logger.LogTrace(AppResources.DeserializedLookupEntitiesWithNumItemsItems, lookupEntities.Count());
                Debug.Assert(lookupEntities.Any(), "Character entities lookup was empty");
            }
        }
        catch (Exception e) when (!Debugger.IsAttached)
        {
            logger.LogCritical(e, AppResources.RanIntoAnErrorWhileDeserializingMessage, e.Message);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            throw;
        }
    }

    private async Task<Dictionary<CsdbProgram, Dictionary<string, string[]>>> DeserializeCheckoutItems(
        CancellationToken token)
    {
        await using var checkoutItemsStream =
            await FileSystem.OpenAppPackageFileAsync("CheckoutItems.json").ConfigureAwait(false);
        await FileSystem.OpenAppPackageFileAsync("CheckoutItems.json").ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<Dictionary<CsdbProgram, Dictionary<string, string[]>>>(checkoutItemsStream, options,
                token)
            .ConfigureAwait(false) ?? throw new InvalidOperationException();
    }

    private async Task<Dictionary<CsdbProgram, Dictionary<string, string>>> DeserializeDocnbrManualFromProgram(
        CancellationToken token)
    {
        await using var docnbrManualFromProgramStream =
            await FileSystem.OpenAppPackageFileAsync("DocnbrManualFromProgram.json").ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<Dictionary<CsdbProgram, Dictionary<string, string>>>(docnbrManualFromProgramStream,
                options, token)
            .ConfigureAwait(false) ?? throw new InvalidOperationException();
    }

    private async Task<Dictionary<string, CsdbProgram[]>> DeserializeProgramPerDocnbr(CancellationToken token)
    {
        await using var programPerDocnbrStream =
            await FileSystem.OpenAppPackageFileAsync("ProgramPerDocnbr.json").ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<Dictionary<string, CsdbProgram[]>>(programPerDocnbrStream, options, token)
            .ConfigureAwait(false) ?? throw new InvalidOperationException();
    }

    private async Task<Dictionary<CsdbProgram, Dictionary<int, UowState>>> DeserializeStatesPerProgram(
        CancellationToken token)
    {
        await using var statesPerProgramStream =
            await FileSystem.OpenAppPackageFileAsync("StatesPerProgram.json").ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<Dictionary<CsdbProgram, Dictionary<int, UowState>>>(statesPerProgramStream, options,
                token)
            .ConfigureAwait(false) ?? throw new InvalidOperationException();
    }

    private async Task<CharacterEntities[]> DeserializeLookupEntities(CancellationToken token)
    {
        await using var lookupEntitiesStream =
            await FileSystem.OpenAppPackageFileAsync("LookupEntities.json").ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<CharacterEntities[]>(lookupEntitiesStream, options, token)
            .ConfigureAwait(false) ?? throw new InvalidOperationException();
    }

    /// <summary>
    ///     Parses the units-of-work states file content asynchronously.
    /// </summary>
    /// <returns>An array of the fully-qualified <see cref="UowState" /> states.</returns>
    internal async Task<UowState[]?> ParseUowContentAsync(CancellationToken token)
    {
        possibleStatesInManual = [];
        var tabIndentation = 0;
        if (string.IsNullOrEmpty(uowContent) || !UowRegex().IsMatch(uowContent) || string.IsNullOrEmpty(Program) ||
            statesPerProgram is null) return null;
        if (StateMatches is null || StateMatches.Count == 0)
            try
            {
                StateMatches = await TryGetUowMatchesAsync(previousUowCts.Token).ConfigureAwait(false);
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.LogError(e, AppResources.InvalidUOWFileUowStatesFileChosenMessage, uowStatesFile, e.Message);
                return null;
            }

        if (StateMatches is null || StateMatches.Count == 0)
        {
            logger.LogError(AppResources.InvalidUOWFileUowStatesFileChosen, uowStatesFile);
            return null;
        }

        var states = new UowState[StateMatches.Count];
        Stack<OrderedDictionary> elementStack = new(StateMatches.Count);
        OrderedDictionary siblingCount = new(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < StateMatches.Count; i++)
        {
            states[i] = new UowState(TagName: StateMatches[i].Groups["tag"].Value);

            var currentIndentation = StateMatches[i].Groups["tabs"].Value.Length;
            if (StateMatches[i].Groups["tabs"].Success)
            {
                if (currentIndentation == tabIndentation + 1)
                {
                    // we went into a child element
                    // my parent is the previous element on the stack
                    elementStack.Push(siblingCount);
                }
                else if (currentIndentation == tabIndentation - 1)
                {
                    // we came out of parent element
                    _ = elementStack.Pop();
                    siblingCount = new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
                }

                // else currentIndentation == tabIndentation // we are at the same level as our predecessor
                SiblingCountIncrement();
                tabIndentation = currentIndentation;
            }
            else
            {
                Debug.WriteLine($"Tabs group was null at count {i}", "Error");
            }

            var parentPath = CalculateParentPath(elementStack);
#pragma warning disable CA1308
            states[i].XPath = $"/{parentPath}/{states[i].TagName}[{siblingCount[states[i].TagName!]}]"
                .ToLowerInvariant();
#pragma warning restore CA1308
            if (StateMatches[i].Groups["key"].Success &&
                !string.IsNullOrWhiteSpace(StateMatches[i].Groups["key"].Value))
            {
                states[i].Key = StateMatches[i].Groups["key"].Value;
#pragma warning disable CA1308
                states[i].XPath =
                    $"{states[i].XPath}[contains(@key,'{states[i].Key}') or contains(@key,'{states[i].Key?.ToLowerInvariant()}')]";
#pragma warning restore CA1308
            }

            if (StateMatches[i].Groups["rs"].Success &&
                !string.IsNullOrWhiteSpace(StateMatches[i].Groups["rs"].Value))
                states[i].Resource = StateMatches[i].Groups["rs"].Value;
            if (StateMatches[i].Groups["title"].Success &&
                !string.IsNullOrWhiteSpace(StateMatches[i].Groups["title"].Value))
                states[i].Title = StateMatches[i].Groups["title"].Value;
            if (StateMatches[i].Groups["lvl"].Success &&
                !string.IsNullOrWhiteSpace(StateMatches[i].Groups["lvl"].Value))
                states[i].Level = StateMatches[i].Groups["lvl"].Value;
            if (StateMatches[i].Groups["state"].Success &&
                !string.IsNullOrWhiteSpace(StateMatches[i].Groups["state"].Value) &&
                int.TryParse(StateMatches[i].Groups["state"].Value, out var stateValue))
            {
                var state = statesPerProgram[csdbProgram][stateValue];
                state.StateValue = states[i].StateValue = stateValue;
                if (!possibleStatesInManual.ContainsKey(stateValue)) possibleStatesInManual.Add(stateValue, state);
                states[i].StateName = state.StateName;
                states[i].Remark = state.Remark;
            }

            continue;

            void SiblingCountIncrement()
            {
                if (siblingCount.Contains(states[i].TagName!) && siblingCount[states[i].TagName!] is int count)
                    // increment siblingCount[states[i].TagName!] as int
                    siblingCount[states[i].TagName!] = count + 1;
                else
                    siblingCount.Add(states[i].TagName!, 1);
            }
        }

        return states;
    }

    /// <summary>
    ///     Calculates the parent XPath using the element stack.
    /// </summary>
    /// <param name="elementStack">The stack of element-names and their corresponding sibling counts.</param>
    /// <returns>The XPath to the parent element.</returns>
    private static string CalculateParentPath(Stack<OrderedDictionary> elementStack)
    {
        StringBuilder builder = new();
        // iterate through the stack without popping elements off; take only the last key-value pair
        // of the ordered dictionary to form the xpath. The key corresponds to the tag name and the value
        // corresponds to the sibling number in the xpath.
        foreach (var orderedSiblingElements in elementStack)
        {
            var tagName = orderedSiblingElements.Keys.Cast<string>().Last();
            if (orderedSiblingElements[tagName] is string count) builder.Insert(0, $"/{tagName}[{count}]");
        }

        return builder.ToString();
    }

    private async Task WriteNodesAsync(IReadOnlyList<XmlNode> nodes, string baseName, string dirPath,
        IProgress<double> progress, CancellationToken token = default)
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            progress.Report(100.0 * (i + 1) / nodes.Count);
            BaXmlDocument xmlFragment = new()
            {
                ResolveEntities = false
            };
            xmlFragment.AppendChild(xmlFragment.ImportNode(nodes[i], true));
            var key = GetFragmentName(nodes[i]);
            var outPath = Path.Combine(dirPath, $"{baseName}-{key}.xml");

            // write the fragment to the outPath
            await Task.Run(async () =>
            {
                try
                {
                    xmlFragment.Save(outPath);
                }
                catch (UnauthorizedAccessException e)
                {
                    logger.LogCritical(e, MauiXmlSplitter.Resources.AppResources.UnableToSaveToWriteToPathOutput, outPath); 
                    ModalOption option = new()
                    {
                        FooterButtonColor = ButtonColor.Danger,
                        FooterButtonText = AppResources.OK,
                        IsVerticallyCentered = true,
                        Message = e.Message,
                        ShowFooterButton = true,
                        Size = ModalSize.Regular,
                        Title = MauiXmlSplitter.Resources.AppResources.UnableToWriteToOutputPath,
                        Type = ModalType.Danger
                    };
                    await modalService.ShowAsync(option);
                    throw new OperationCanceledException(token);
                }
            }, token).ConfigureAwait(false);

            logger.LogInformation(AppResources.WroteFragmentName, Path.GetFileName(outPath));
        }
    }

    /// <summary>
    /// Constructs the name of the fragment using the <c>key</c> attribute.
    /// Where there is no <c>key</c> attribute, the <see cref="MD5"/> of the hash code is computed.
    /// </summary>
    /// <param name="xmlNode">The XML node.</param>
    /// <returns>The <c>key</c> attribute or the <see cref="MD5"/> of the hash code.</returns>
    public static string GetFragmentName(XmlNode xmlNode) => xmlNode.Attributes?["key"]?.Value ?? StripNonWordChars()
        .Replace(Convert.ToBase64String(MD5.HashData(BitConverter.GetBytes(xmlNode.GetHashCode()))),
            string.Empty);

    public bool IsReadyVerbose(IProgress<double> progress)
    {
        progress.Report(0);
        if (string.IsNullOrEmpty(uowContent))
        {
            logger.LogWarning(AppResources.AttemptedSplitNotReadyUnitOfWorkStatesNotProvided);
            return false;
        }

        if (string.IsNullOrEmpty(Program))
        {
            logger.LogWarning(AppResources.AttemptedSplitNotReadyProgramNotProvided);
            return false;
        }

        if (xmlSourceFile is null && string.IsNullOrEmpty(xmlContent))
        {
            logger.LogWarning(AppResources.AttemptedSplitNotReadyXMLNotProvided);
            return false;
        }

        if (string.IsNullOrEmpty(outputDirectory))
        {
            logger.LogWarning(AppResources.AttemptedSplitNotReadyOutputDirectoryNotProvided);
            return false;
        }

        // check if outputDirectory exists, if not, create it
        if (!Directory.Exists(outputDirectory))
        {
            logger.LogTrace(AppResources.CreatingOutputDirectoryOutputDirectory, outputDirectory);
            try
            {
                _ = Directory.CreateDirectory(outputDirectory);
                logger.LogTrace(AppResources.CreatedOutputDirectoryOutputDirectory, outputDirectory);
            }
            catch (IOException ex) when (!Debugger.IsAttached)
            {
                logger.LogError(ex, AppResources.AnIOErrorOccurredWhileCreatingADirectoryAtOutputDirectory,
                    outputDirectory);
                return false;
            }
            catch (UnauthorizedAccessException ex) when (!Debugger.IsAttached)
            {
                logger.LogError(ex, AppResources.YouDoNotHavePermissionToCreateADirectoryAtOutputDirectory,
                    outputDirectory);
                return false;
            }
            catch (ArgumentException ex) when (!Debugger.IsAttached)
            {
                logger.LogError(ex, AppResources.TheDirectoryPathOutputDirectoryIsInvalid, outputDirectory);
                return false;
            }
            catch (NotSupportedException ex) when (!Debugger.IsAttached)
            {
                logger.LogError(ex, AppResources.TheDirectoryPathFormatOutputDirectoryIsNotSupported, outputDirectory);
                return false;
            }

            if (possibleStatesInManual is null || xmlContent is null)
            {
                logger.LogError(AppResources.UOWNotParsed);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Executes the split.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    /// <param name="progress"></param>
    /// <param name="token"></param>
    public async Task<bool> ExecuteSplit(object sender, EventArgs e, IProgress<double> progress,
        CancellationToken token)
    {
        if(report is XmlSplitReport bagOfReport) bagOfReport.Clear();
        if (!IsReadyVerbose(progress) || UowStates?.Any() != true)
            return false;
        logger.LogInformation(AppResources.FoundNumUnitsOfWorkInTheManual, UowStates.Count());

        try
        {
            await Task.Run(() => { xml = new BaXmlDocument(); xml.LoadXml(xmlContent!);}, token).ConfigureAwait(false);
        }
        catch (XmlException xmlException)
        {
            logger.LogCritical(xmlException, AppResources.XMLLoadFailedWithMessage, xmlException.Message);
            ModalOption option = new()
            {
                FooterButtonColor = ButtonColor.Danger,
                FooterButtonText = AppResources.OK,
                IsVerticallyCentered = true,
                Message = xmlException.Message,
                ShowFooterButton = true,
                Size = ModalSize.Regular,
                Title = AppResources.ErrorLoadingXML,
                Type = ModalType.Danger
            };
            await modalService.ShowAsync(option);
        }

        // Compare the docnbr to UowStatesDocnbr
        if (xml.DocumentElement?.GetAttribute("docnbr") is not { } docnbr)
        {
            logger.LogError(AppResources.TheXMLHadNoDocnbrAttribute);
            return false;
        }

        if (docnbr != UowStatesDocnbr)
        {
            logger.LogWarning(AppResources.TheXMLDocnbrDocnbrDoesNotMatchTheUOWDocnbrUowStatesDocnbr, docnbr,
                UowStatesDocnbr);
            return false;
        }

        if (checkoutItems is null)
        {
            logger.LogError(AppResources.CheckoutItemsAreNull);
            return false;
        }

        if (manualFromDocnbr is null)
        {
            logger.LogError(AppResources.ManualFromDocnbrIsNull);
            return false;
        }

        if (await GetXmlNodesAsync().ConfigureAwait(false) is not { } nodes || nodes.Length == 0)
        {
            logger.LogError(AppResources.XMLNodesWereEmptyNoCheckoutUnitsToExport);
            return false;
        }

        try
        {
            #region WriteXmlFragments
            await WriteNodesAsync(nodes, XmlSourceFileBaseName, OutputDirectory, progress,
                    token).ConfigureAwait(false); 
            #endregion WriteXmlFragments

            report.BaseName = XmlSourceFileBaseName;

            #region CheckoutNodeBuilding
            /* Populate checkout node children for report building */
            Dictionary<XmlNode, List<StateWithNode>> childrenPerCheckoutItem = new(nodes.Length);
            // ReSharper disable once MergeIntoPattern
            if (FullyQualifiedSelectedStates is null || nodes.Length == 0 || (FullyQualifiedSelectedStates.Count() is { } statesCount && statesCount == 0)) throw new OperationCanceledException(token);
            for (var i = 0; i < statesCount; i++)
            {
                progress.Report(100.0 * (i + 1) / statesCount);
                if (FullyQualifiedSelectedStates.ElementAt(i) is not { XPath: { } xpath } state ||
                    xml.SelectSingleNode(xpath) is not { } child) continue;
                if (nodes.FirstOrDefault(possibleParent => SplittingReport.IsDescendant(possibleParent, child)) is not
                    { } parent)
                {
                    Debug.WriteLineIf(await GetCheckoutElementNamesAsync() is {} checkoutElementNames && checkoutElementNames.Contains(FullyQualifiedSelectedStates.ElementAt(i).TagName),$"Could not find the parent node for state {state}","Error");
                    continue;
                }
                if(childrenPerCheckoutItem.TryGetValue(parent, out var children))
                    children.Add(new StateWithNode(state, child));
                else
                    childrenPerCheckoutItem.Add(parent, [new StateWithNode(state,child)]);
            }
            #endregion CheckoutNodeBuilding

            #region ReportEntryBuilding
            for (var i = 0; i < childrenPerCheckoutItem.Keys.Count; i++)
            {
                progress.Report(100.0 * (i + 1) / childrenPerCheckoutItem.Keys.Count);
                var parent = childrenPerCheckoutItem.Keys.ElementAt(i);
                report.AddEntries(childrenPerCheckoutItem[parent],parent,i+1);
            }
            #endregion ReportEntryBuilding

            #region SaveReport

            var reportTypes = Enum.GetValuesAsUnderlyingType<XmlSplitReport.ReportFormat>()
                .Cast<XmlSplitReport.ReportFormat>().ToArray();
            for (var i = 0; i < reportTypes.Length; i++)
            {
                progress.Report(100.0 * (i + 1) / childrenPerCheckoutItem.Keys.Count);
                if (Enum.GetName(reportTypes[i]) is not { } extension)
                {
                    continue;
                }
                var outPath = Path.Join(OutputDirectory, $"{report.Name}.{extension.ToLowerInvariant()}");
                await report.SaveAsync(outPath, reportTypes[i]).ConfigureAwait(false);
                logger.LogInformation(AppResources.WritingSplitReportAsFormat, extension);
            }

            #endregion SaveReport
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(AppResources.SavingFragmentsCancelled);
            return false;
        }
        finally
        {
            progress.Report(1);
        }
        logger.LogInformation(AppResources.SplittingXMLFileXmlFilePathIntoNumNodesFragments, xmlSourceFile,
            nodes.Length.ToString());
        return true;
    }

    [GeneratedRegex("""\W*""")]
    private static partial Regex StripNonWordChars();
}

/// <summary>
/// Struct to correlate <see cref="UowState"/> state with <see cref="XmlNode"/> node
/// </summary>
/// <seealso cref="System.IEquatable&lt;StateWithNode&gt;" />
public record struct StateWithNode(UowState State, XmlNode Child)
{
    /// <summary>
    /// Performs an implicit conversion from <see cref="MauiXmlSplitter.Models.StateWithNode"/> to <see cref="(MauiXmlSplitter.Models.UowState state, System.Xml.XmlNode child)"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator (UowState state, XmlNode child)(StateWithNode value)
    {
        return (value.State, value.Child);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="(MauiXmlSplitter.Models.UowState State, System.Xml.XmlNode Child)"/> to <see cref="MauiXmlSplitter.Models.StateWithNode"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator StateWithNode((UowState State, XmlNode Child) value)
    {
        return new StateWithNode(value.State, value.Child);
    }
}