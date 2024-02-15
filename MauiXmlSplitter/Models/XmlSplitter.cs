using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using BlazorBootstrap;
using F23.StringSimilarity;
using Microsoft.Extensions.Logging;
using static MauiXmlSplitter.Models.CsdbContext;
using Path = System.IO.Path;

namespace MauiXmlSplitter.Models;

public partial class XmlSplitter(ILogger logger, ModalService modalService)
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

    /// <summary>The timestamp format.</summary>
    internal const string LogTimestampFormat = "HH:mm:ss.fffffff";

    /// <summary>The report timestamp format</summary>
    internal const string ReportTimestampFormat = "yyyy - MM - dd - HH - mm - ss - fffffff";

    /// <summary>The CSDB programs.</summary>
    internal static readonly string[] Programs =
        Enum.GetNames<CsdbProgram>().Where(e => e != Enum.GetName(CsdbProgram.None)).ToArray();

    /// <summary>
    ///     The <see cref="F23.StringSimilarity.Jaccard" /> object for performing string similarity calculations.
    /// </summary>
    internal static readonly Jaccard Jaccard = new(2);

    private readonly JsonSerializerOptions options = new();

    /// <summary>The XML object that will parse the XML content. </summary>
    /// <seealso cref="BaXmlDocument" />
    private readonly BaXmlDocument xml = new()
    {
        ResolveEntities = false
    };

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

    /// <summary>
    ///     The XPath string that would be used to select the nodes to split from the XML. This is calculated from the tag
    ///     names and key values in the UOW states file.
    /// </summary>
    private string? xpath;

    /// <summary>The fully populated unit-of-work states that are selected by the user for export.</summary>
    public IEnumerable<UowState>? FullyQualifiedSelectedStates { get; set; }

    public Hashtable PossibleStatesInManual => possibleStatesInManual ?? [];

    public string UowContent
    {
        get => uowContent ?? string.Empty;
        set => uowContent = value;
    }

    public string XmlContent
    {
        get => xmlContent ?? string.Empty;
        set => xmlContent = value;
    }

    /// <summary>
    ///     Gets the XPath.
    /// </summary>
    /// <value>
    ///     The XPath that would select the nodes for export.
    /// </value>
    public string XPath => xpath ?? string.Empty;

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
    internal string XmlSourceFileBaseName => XmlFilenameRe()
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

    private bool IsLoadingXml { get; set; }

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
        const int numLines = 5; // number of lines to check provisionally
        var isUowText =
            (Success: true, Reason: ReasonForUowFailure.None); // assume file is text until assumption is falsified
        await Task.Run(CheckLines);
        return isUowText;

        bool CheckLineIsPlaintext(in string line)
        {
            var isPlaintext = true;
            for (var j = 0; j < line.Length && isPlaintext; j++)
                isPlaintext =
                    line[j] == '\t' || !char.IsControl(line[j]); // Control characters indicate file is not plaintext
            return isPlaintext;
        }

        void CheckLines()
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
        logger.LogInformation("Closest manual found for {Program} is {ClosestManual}", Program, closestManual);
        return checkoutItems[csdbProgram][closestManual];
    }

    private async Task<XmlNode[]> GetXmlNodesAsync()
    {
        if (!xml.HasChildNodes || string.IsNullOrEmpty(xpath))
            return [];
        var checkoutElementNames = await GetCheckoutElementNamesAsync().ConfigureAwait(false);
        if (checkoutElementNames.Length == 0)
        {
            if (await Task.Run(() => xml.SelectNodes(xpath)).ConfigureAwait(false) is { } plainNodes)
                return plainNodes.Cast<XmlNode>().ToArray();
            return [];
        }

        if (await Task.Run(() => xml.SelectNodesByCheckout(xpath, checkoutElementNames)).ConfigureAwait(false) is
            { } checkoutNodes)
            return checkoutNodes;
        return [];
    }

    [GeneratedRegex("[_-]$")]
    private static partial Regex TerminusCharPattern();

    /// <summary>The XML filename regular expression pattern.</summary>
    [GeneratedRegex(@"(?<basename>[\w_-]+[\d-]{8,}).*", RegexOptions.Compiled | RegexOptions.Multiline, 15 * 1000)]
    internal static partial Regex XmlFilenameRe();

    [SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances",
        Justification =
            "We deserialize this exactly once on start-up, so it would be overkill to cache the JsonSerializerOptions.")]
    public async Task LoadAssets(CancellationToken token = default)
    {
        try
        {
            // add the custom converters to the JsonSerializerOptions options

            foreach (var converterType in new Type[]
                         { typeof(CsdbProgramConverter), typeof(UowStateConverter), typeof(CharacterEntitiesConverter) })
            {
                if (options.Converters.All(c => c.GetType() != converterType) &&
                    Activator.CreateInstance(converterType) as JsonConverter is { } converter)
                    options.Converters.Add(converter);
            }

            if (checkoutItems is not { Count: > 0 })
            {
                checkoutItems = await DeserializeCheckoutItems(token);
                logger.LogInformation("Deserialized CheckoutItems with {NumItems} items", checkoutItems.Count);
                Debug.Assert(checkoutItems is { Count: > 0 }, "Checkout items were empty");
            }

            if (manualFromDocnbr is not { Count: > 0 })
            {
                manualFromDocnbr = await DeserializeDocnbrManualFromProgram(token);
                logger.LogInformation("Deserialized DocnbrManualFromProgram with {NumItems} items",
                    manualFromDocnbr.Count);
                Debug.Assert(manualFromDocnbr is { Count: > 0 }, "Manual from docnbr was empty");
            }

            if (programPerDocnbr is not { Count: > 0 })
            {
                programPerDocnbr = await DeserializeProgramPerDocnbr(token);
                logger.LogInformation("Deserialized ProgramPerDocnbr with {NumItems} items", programPerDocnbr.Count);
                Debug.Assert(programPerDocnbr is { Count: > 0 }, "Program per docnbr was empty");
            }

            if (statesPerProgram is not { Count: > 0 })
            {
                statesPerProgram = await DeserializeStatesPerProgram(token);
                logger.LogInformation("Deserialized StatesPerProgram with {NumItems} items", statesPerProgram.Count);
                Debug.Assert(statesPerProgram is { Count: > 0 }, "States per program was empty");
            }

            if (lookupEntities?.Any() != false)
            {
                lookupEntities = await DeserializeLookupEntities(token);
                logger.LogInformation("Deserialized LookupEntities with {NumItems} items", lookupEntities.Count());
                Debug.Assert(lookupEntities.Any(), "Character entities lookup was empty");
            }
        }
        catch (Exception e) when (!Debugger.IsAttached)
        {
            logger.LogCritical($"Ran into an error while deserializing: '{e.Message}'");
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
    /// <returns>An array if the <see cref="UowState" /> states.</returns>
    internal async Task<UowState[]?> ParseUowContentAsync(CancellationToken token)
    {
        possibleStatesInManual = [];
        var tabIndentation = 0;
        if (string.IsNullOrEmpty(uowContent) || !UowRegex().IsMatch(uowContent) || string.IsNullOrEmpty(Program) ||
            statesPerProgram is null) return null;
        if (StateMatches is null || StateMatches.Count == 0)
            try
            {
                StateMatches = await TryGetUowMatchesAsync(previousUowCts.Token);
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.LogError("Invalid UOW file '{UowStatesFile}' chosen", uowStatesFile);
                return null;
            }

        if (StateMatches is null || StateMatches.Count == 0)
        {
            logger.LogError("Invalid UOW file '{UowStatesFile}' chosen", uowStatesFile);
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
        progress.Report(0);
        if (string.IsNullOrEmpty(uowContent))
        {
            logger.LogWarning("Attempted split not ready: unit-of-work states not provided");
            return false;
        }

        if (string.IsNullOrEmpty(Program))
        {
            logger.LogWarning("Attempted split not ready: program not provided");
            return false;
        }

        if (xmlSourceFile is null && string.IsNullOrEmpty(xmlContent))
        {
            logger.LogWarning("Attempted split not ready: XML not provided");
            return false;
        }

        if (string.IsNullOrEmpty(outputDirectory))
        {
            logger.LogWarning("Attempted split not ready: output directory not provided");
            return false;
        }

        // check if outputDirectory exists, if not, create it
        if (!Directory.Exists(outputDirectory))
        {
            logger.LogTrace("Creating output directory: '{OutputDirectory}'", outputDirectory);
            try
            {
                _ = Directory.CreateDirectory(outputDirectory);
                logger.LogTrace("Created output directory: '{OutputDirectory}'", outputDirectory);
            }
            catch (IOException ex) when (!Debugger.IsAttached)
            {
                logger.LogError(ex, "An I/O error occurred while creating a directory at '{OutputDirectory}'",
                    outputDirectory);
                return false;
            }
            catch (UnauthorizedAccessException ex) when (!Debugger.IsAttached)
            {
                logger.LogError(ex, "You do not have permission to create a directory at '{OutputDirectory}'",
                    outputDirectory);
                return false;
            }
            catch (ArgumentException ex) when (!Debugger.IsAttached)
            {
                logger.LogError(ex, "The directory path '{OutputDirectory}' is invalid", outputDirectory);
                return false;
            }
            catch (NotSupportedException ex) when (!Debugger.IsAttached)
            {
                logger.LogError(ex, "The directory path format '{OutputDirectory}' is not supported", outputDirectory);
                return false;
            }
        }

        if (await ParseUowContentAsync(token) is not { } states || possibleStatesInManual is null || xmlContent is null)
        {
            logger.LogError("UOW not parsed");
            return false;
        }

        try
        {
            await Task.Run(() => xml.LoadXml(xmlContent), token).ConfigureAwait(false);
        }
        catch(XmlException xmlException)
        {
            logger.LogCritical("XML load failed with '{message}'",xmlException.Message);
            ModalOption option = new()
            {
                FooterButtonColor = ButtonColor.Danger,
                FooterButtonText = "OK",
                IsVerticallyCentered = true,
                Message = xmlException.Message,
                ShowFooterButton = true,
                Size = ModalSize.Regular,
                Title = "Error loading XML",
                Type = ModalType.Danger
            };
            await modalService.ShowAsync(option);
        }
        // Compare the docnbr to UowStatesDocnbr
        if (xml.DocumentElement?.GetAttribute("docnbr") is not { } docnbr)
        {
            logger.LogError("The XML had no docnbr attribute");
            return false;
        }

        if (docnbr != UowStatesDocnbr)
        {
            logger.LogWarning("The XML docnbr '{Docnbr}' does not match the UOW docnbr '{UowStatesDocnbr}'", docnbr,
                UowStatesDocnbr);
            return false;
        }

        if (checkoutItems is null || manualFromDocnbr is null)
        {
            logger.LogError("Checkout items or manual from docnbr are null");
            return false;
        }

        if (await GetXmlNodesAsync().ConfigureAwait(false) is not { } nodes || nodes.Length == 0)
        {
            logger.LogError("XML nodes were empty");
            return false;
        }

        logger.LogInformation("Splitting XML file {XmlFilePath} into {NumNodes} fragments", xmlSourceFile,
            nodes.Length.ToString());
        progress.Report(1);
        return true;
    }

    public async Task LoadXml(object sender, EventArgs e, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(xmlSourceFile))
            return;
        IsLoadingXml = true;
        xmlContent = await File.ReadAllTextAsync(xmlSourceFile, token).ConfigureAwait(false);
        if (string.IsNullOrEmpty(xmlContent))
            throw new IOException("XML content empty after file read.");
        await Task.Run(() => xml.LoadXml(xmlContent), token).ConfigureAwait(false);
        IsLoadingXml = false;
    }
}