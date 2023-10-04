using BaXmlSplitter.Properties;
using F23.StringSimilarity;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace BaXmlSplitter;

/// <summary>
/// Collection of static methods and fields that aid the <see cref="XmlSplitter" />.
/// </summary>
internal static class XmlSplitterHelpers
{
    /// <summary>The default output directory.</summary>
    internal const string DefaultOutputDir = "WIP";
    /// <summary>The timestamp format.</summary>
    internal const string TimestampFormat = "HH:mm:ss.fffffff";
    /// <summary>The CSDB programs.</summary>
    internal static readonly string[] Programs = Enum.GetNames<CsdbProgram>();
    /// <summary>The regular expression timeout.</summary>
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);
    /// <summary>The regular expression options.</summary>
    internal const RegexOptions ReOptions = RegexOptions.Compiled | RegexOptions.Multiline;
    /// <summary>The UOW states file parsing regular expression pattern.</summary>
    internal const string UowPattern = """(?<tabs>\t*)(?:Front Matter: )?(?<tag>\S+)(?: (?<key>\S+))?(?: (?<rs>RS-\d+))?(?: - (?<title>.+?))?(?: (?<lvl>[A-Z0-9 =]+?))? +-- .*?\(state = "(?<state>[^"]*)"\)$""";
    /// <summary>The UOW states file regular expression
    /// object.</summary>
    internal static readonly Regex UowRegex = new(UowPattern, ReOptions, Timeout);
    /// <summary>The XML filename pattern.</summary>
    internal const string XmlFilenamePattern = @"([\w_-]+[\d-]{8,}).*";
    /// <summary>The XML filename regular expression pattern.</summary>
    internal static readonly Regex XmlFilenameRe = new(XmlFilenamePattern, ReOptions, Timeout);
    /// <summary>The newline strings for Unix and Windows systems.</summary>
    internal static readonly string[] Newlines = ["\r\n", "\n"];
    /// <summary>
    /// The xpath separators
    /// </summary>
    internal static readonly string[] XpathSeparators = ["|", " or "];
    /// <summary>
    /// The <see cref="F23.StringSimilarity.Jaccard"/> object for performing string similarity calculations.
    /// </summary>
    internal static readonly Jaccard Jaccard = new(k: 2);
    /// <summary>
    /// <para>The priority queue for automatically ordering string similarity results.</para>
    /// <para>This will be used for finding closest matching manual name in the <see cref="XmlSplitter.checkoutItems"/> keys for each manual type in the <see cref="XmlSplitter.manualFromDocnbr"/> dictionary.</para>
    /// </summary>
    internal static readonly PriorityQueue<string, double> BestMatch = new();

    /// <summary>
    /// Font families loaded by <see cref="XmlSplitter.LoadFonts"/>.
    /// </summary>
    internal enum FontFamilies
    {
        /// <summary>
        /// The 72 font
        /// </summary>
        _72,
        /// <summary>
        /// The 72 black font
        /// </summary>
        _72_Black,
        /// <summary>
        /// The 72 condensed font
        /// </summary>
        _72_Condensed,
        /// <summary>
        /// The 72 light font
        /// </summary>
        _72_Light,
        /// <summary>
        /// The 72 monospace font
        /// </summary>
        _72_Monospace
    };
    /// <summary>
    /// The CSDB CsdbProgram.
    /// </summary>
    internal enum CsdbProgram
    {
        /// <summary>
        /// The <c>B_IFM</c> program for instrument flight manuals
        /// </summary>
        // ReSharper disable once InconsistentNaming
        B_IFM,
        /// <summary>
        /// The <c>CH604PROD</c> program for Challenger 6XX maintenance manuals
        /// </summary>
        // ReSharper disable once InconsistentNaming
        CH604PROD,
        /// <summary>
        /// The <c>CTALPROD</c> program for Challenger 3XX maintenance manuals
        /// </summary>
        // ReSharper disable once InconsistentNaming
        CTALPROD,
        /// <summary>
        /// The <c>GXPROD</c> program for Global and Global Express maintenance manuals
        /// </summary>
        // ReSharper disable once InconsistentNaming
        GXPROD,
        /// <summary>
        /// The <c>LJ4045PROD</c> program for Learjet 40/45 maintenance manuals
        /// </summary>
        // ReSharper disable once InconsistentNaming
        LJ4045PROD
    };

    #region BinaryCheck
    /// <summary>
    /// Determines whether the specified file at the path is binary.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if the specified path is binary; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsBinary(string path)
    {
        FileInfo fi = new(path);
        var length = fi.Length;
        if (length == 0) return false;

        using StreamReader stream = new(path);
        int ch;
        while ((ch = stream.Read()) != -1)
        {
            if (IsControlChar(ch))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Control characters in ASCII used to detect binary files
    /// </summary>
    internal enum ControlChars
    {
        /// <summary>
        /// The null character
        /// </summary>
        Nul = (char)0,
        /// <summary>
        /// The backspace character
        /// </summary>
        Bs = (char)8,
        /// <summary>
        /// The carriage return character
        /// </summary>
        Cr = (char)13,
        /// <summary>
        /// The substitute character
        /// </summary>
        Sub = (char)26
    }
    /// <summary>
    /// Determines whether [is control character] [the specified character ch].
    /// </summary>
    /// <param name="ch">The character to check.</param>
    /// <returns>
    ///   <c>true</c> if [is control character] [the specified ch]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsControlChar(int ch)
    {
        return ch is > (int)ControlChars.Nul and < (int)ControlChars.Bs or > (int)ControlChars.Cr and < (int)ControlChars.Sub;
    }
    #endregion BinaryCheck
    /// <summary>
    ///   <para>
    /// Deserializes the checkout items from PowerShell CLI XML.
    /// The following PowerShell is used to first create the checkout items as CLI XML:</para>
    ///   <span id="cbc_1" codelanguage="PowerShell" x-lang="PowerShell">
    ///     <div class="highlight-title">
    ///       <span tabindex="0" class="highlight-copycode"></span>PowerShell</div>
    ///     <div class="code">
    ///       <pre xml:space="preserve">  Get-ChildItem -Path $env:TEMP\checkoutuow | ForEach-Object -Begin { $checkoutItems = [System.Collections.Generic.Dictionary[<span class="highlight-keyword">string</span>,<span class="highlight-keyword">string</span>[]]]::<span class="highlight-keyword">new</span>() } -Process { $checkout = ($_ | Select-String -Pattern "([^\s\)\(]+)(?=.*\(checkout\))").Matches.Value | Sort-Object -Unique; $elements = ($_.BaseName | Select-String -Pattern '[^\s\(\)]+' -AllMatches).Matches.Value;  $elements | ForEach-Object { $checkoutItems.Add($_,$checkout); } } -End { $checkoutItems } | Export-Clixml -Path $env:OneDriveCommercial\checkoutUowItems.xml
    /// $checkoutItemsPerProgram = [System.Collections.Generic.Dictionary[<span class="highlight-keyword">string</span>,hashtable]]::<span class="highlight-keyword">new</span>()
    /// $checkoutItemsPerProgram['B_IFM'] = $checkoutItems</pre>
    ///     </div>
    ///   </span>
    /// </summary>
    /// <returns>Returns a Dictionary that maps <see cref="CsdbProgram" /> to a Dictionary where the key is the manual name and the value is an array of the element names eligible for checkout.</returns>
    internal static Dictionary<CsdbProgram, Dictionary<string, string[]>>? DeserializeCheckoutItems()
    {
        dynamic deserializedCheckoutUowItems = PSSerializer.Deserialize(Resources.CheckoutItems);
        Dictionary<CsdbProgram, Dictionary<string, string[]>> programCheckoutItems = new(Programs.Length);
        foreach (var program in Enum.GetNames<CsdbProgram>())
        {
            ICollection docnbrs = deserializedCheckoutUowItems[program].Keys;
            Dictionary<string, string[]>? checkoutUowItems = new(docnbrs.Count);
            foreach (string docnbr in docnbrs)
            {
                var deserializedUowNames = deserializedCheckoutUowItems[program][docnbr];
                if (deserializedUowNames.ToArray(typeof(string)) is string[] uowNames)
                {
                    checkoutUowItems.Add(docnbr, value: uowNames);
                }
            }
            programCheckoutItems.Add(Enum.Parse<CsdbProgram>(program), checkoutUowItems);
        }
        return programCheckoutItems;
    }
    /// <summary>
    /// Deserializes the UOW states from PowerShell CLI XML.
    /// </summary>
    /// <returns>Returns a dictionary that maps <see cref="CsdbProgram"/> to a Dictionary where the key is the state value (int) and the value is the details of the work state as a <see cref="UowState"/> object.</returns>
    internal static Dictionary<CsdbProgram, Dictionary<int, UowState>>? DeserializeStates()
    {
        dynamic deserializedStatesPerProgram = PSSerializer.Deserialize(Resources.StatesPerProgramXml);
        ICollection programs = deserializedStatesPerProgram.Keys;
        Dictionary<CsdbProgram, Dictionary<int, UowState>> statesPerProgram = new(programs.Count);
        foreach (var program in programs.Cast<string>())
        {
            var stateNames = deserializedStatesPerProgram[program];
            Dictionary<int, UowState> states = new(stateNames.Count);
            foreach (int stateValue in stateNames.Keys)
            {
                var stateNameAndRemark = stateNames[stateValue];
                UowState uowState = new(StateValue: stateValue, StateName: stateNameAndRemark.statename, Remark: stateNameAndRemark.remark);
                states.Add(stateValue, uowState);
            }
            statesPerProgram.Add(Enum.Parse<CsdbProgram>(program), states);
        }
        return statesPerProgram;
    }
    /// <summary>
    /// Deserializes the Dictionary to get the manual type from the docnbr per program.
    /// </summary>
    /// <returns>Returns a dictionary that maps <see cref="CsdbProgram"/> to a Dictionary where the key is the docnbr (string) and the value is the manual type (string).</returns>
    internal static Dictionary<CsdbProgram, Dictionary<string, string>>? DeserializeDocnbrManualFromProgram()
    {
        dynamic deserializedDocnbrManualFromProgram = PSSerializer.Deserialize(Resources.DocnbrManualFromProgram);
        ICollection programs = deserializedDocnbrManualFromProgram.Keys;
        Dictionary<CsdbProgram, Dictionary<string, string>> docnbrManualFromProgram = new(programs.Count);
        foreach (var program in programs.Cast<string>())
        {
            Dictionary<string, string> manualFromDocnbr = new(deserializedDocnbrManualFromProgram[program].Count);
            foreach (string docnbr in deserializedDocnbrManualFromProgram[program].Keys)
            {
                var manual = deserializedDocnbrManualFromProgram[program][docnbr];
                manualFromDocnbr.Add(docnbr, manual);
            }
            docnbrManualFromProgram.Add(Enum.Parse<CsdbProgram>(program), manualFromDocnbr);
        }
        return docnbrManualFromProgram;
    }
    /// <summary>
    /// Determines whether the specified parent node is an ancestor of the possible child node.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="possibleChild">The node that may be a child of the parent.</param>
    /// <returns>
    ///   <c>true</c> if the specified parent is descendant; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsDescendant(XmlNode parent, XmlNode possibleChild)
    {
        if (parent.Equals(possibleChild))
        {
            return true; // possibleChild is the same node as parent
        }
        else if (possibleChild.ParentNode is null)
        {
            return false; // possibleChild has no parent
        }
        else
        {
            return IsDescendant(parent, possibleChild.ParentNode); // recursively check parent's parent
        }
    }
    /// <summary>
    /// Calculates the parent tag.
    /// </summary>
    /// <param name="curNode">The current node.</param>
    /// <returns></returns>
    internal static XmlNode CalculateParentTag(XmlNode curNode)
    {
        if (curNode is XmlDocument { ChildNodes: { } children } && children.Cast<XmlNode>() is { } childrenList && childrenList.FirstOrDefault(predicate: child => child.NodeType is not XmlNodeType.Comment and not XmlNodeType.DocumentType) is { } result)
        {
            return result;
        }
        else if /* has key attribute */ (curNode.ParentNode?.Attributes != null && curNode.ParentNode.Attributes.Cast<XmlAttribute>().Any(attribute => attribute.Name.Equals("key", StringComparison.OrdinalIgnoreCase)))
        {
            return curNode.ParentNode;
        }
        else /* does not have key attribute; need to recurse */
        {
            return CalculateParentTag(curNode.ParentNode!);
        }
    }
    /// <summary>
    /// Gets the docnbr implied by the UOW states file.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The docnbr (string) implied by the first line of the UOW states file content.</returns>
    internal static string GetUowStatesDocnbr(ref string content)
    {
        return content.Split(Newlines, StringSplitOptions.None)[0];
    }
    /// <summary>
    /// Parses the content of the units-of-work states file.
    /// </summary>
    /// <param name="uowContent">Content of the UOW states file.</param>
    /// <param name="programStr">The program as string.</param>
    /// <param name="statesPerProgram">The states per program Dictionary.</param>
    /// <param name="uowStatesFile">The UOW states file path.</param>
    /// <param name="logMessages">The log messages to return to the caller.</param>
    /// <param name="statesInManual">The UOW states that actually appear in the current manual.</param>
    /// <param name="foundDocnbr">The found docnbr in the UOW states file.</param>
    /// <returns>
    /// An array of <see cref="UowState" /> objects corresponding to the states per node indicated by the states file. May be null if parsing fails.
    /// </returns>
    internal static UowState[]? ParseUowContent(string? uowContent, string? programStr, Dictionary<CsdbProgram, Dictionary<int, UowState>>? statesPerProgram, string? uowStatesFile, out List<LogMessage> logMessages, out Hashtable statesInManual, out string foundDocnbr)
    {
        UowState[] states;
        statesInManual = [];
        logMessages = [];
        var tabIndentation = 0;
        if (!string.IsNullOrEmpty(uowContent) && UowRegex.IsMatch(uowContent) && !string.IsNullOrEmpty(programStr) && statesPerProgram is not null && Enum.Parse<CsdbProgram>(programStr) is var program)
        {
            foundDocnbr = GetUowStatesDocnbr(ref uowContent);
            var stateMatches = UowRegex.Matches(uowContent);
            if (stateMatches.Count == 0)
            {
                logMessages.Add(new LogMessage($"Invalid UOW file '{uowStatesFile}' chosen", Severity.Error));
                return null;
            }
            states = new UowState[stateMatches.Count];
            Stack<OrderedDictionary> elementStack = new(stateMatches.Count);
            OrderedDictionary siblingCount = new(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < stateMatches.Count; i++)
            {
                states[i] = new UowState(TagName: stateMatches[i].Groups["tag"].Value);

                var currentIndentation = stateMatches[i].Groups["tabs"].Value.Length;
                if (stateMatches[i].Groups["tabs"].Success)
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
                states[i].XPath = $"/{parentPath}/{states[i].TagName}[{siblingCount[states[i].TagName!]}]".ToLowerInvariant();
                if (stateMatches[i].Groups["key"].Success && !string.IsNullOrWhiteSpace(stateMatches[i].Groups["key"].Value))
                {
                    states[i].Key = stateMatches[i].Groups["key"].Value;
                    states[i].XPath = $"{states[i].XPath}[contains(@key,'{states[i].Key}') or contains(@key,'{states[i].Key?.ToLowerInvariant()}')]";
                }
                if (stateMatches[i].Groups["rs"].Success && !string.IsNullOrWhiteSpace(stateMatches[i].Groups["rs"].Value))
                {
                    states[i].Resource = stateMatches[i].Groups["rs"].Value;
                }
                if (stateMatches[i].Groups["title"].Success && !string.IsNullOrWhiteSpace(stateMatches[i].Groups["title"].Value))
                {
                    states[i].Title = stateMatches[i].Groups["title"].Value;
                }
                if (stateMatches[i].Groups["lvl"].Success && !string.IsNullOrWhiteSpace(stateMatches[i].Groups["lvl"].Value))
                {
                    states[i].Level = stateMatches[i].Groups["lvl"].Value;
                }
                if (stateMatches[i].Groups["state"].Success && !string.IsNullOrWhiteSpace(stateMatches[i].Groups["state"].Value) && int.TryParse(stateMatches[i].Groups["state"].Value, out var stateValue))
                {
                    var state = statesPerProgram[program][stateValue];
                    state.StateValue = states[i].StateValue = stateValue;
                    if (!statesInManual.ContainsKey(stateValue))
                    {
                        statesInManual.Add(stateValue, state);
                    }
                    states[i].StateName = state.StateName;
                    states[i].Remark = state.Remark;
                }
                continue;

                void SiblingCountIncrement()
                {
                    if (siblingCount.Contains(states[i].TagName!) && siblingCount[states[i].TagName!] is int count)
                    {
                        // increment siblingCount[states[i].TagName!] as int
                        siblingCount[states[i].TagName!] = count + 1;
                    }
                    else
                    {
                        siblingCount.Add(states[i].TagName!, 1);
                    }
                }
            }
            return states;
        }
        else
        {
            foundDocnbr = "";
            return null;
        }
    }

    /// <summary>
    /// Calculates the parent XPath using the element stack.
    /// </summary>
    /// <param name="elementStack">The stack of element-names and their corresponding sibling counts.</param>
    /// <returns>The XPath to the parent element.</returns>
    private static string CalculateParentPath(Stack<OrderedDictionary> elementStack)
    {
        StringBuilder builder = new();
        // iterate through the stack without popping elements off; take only the last key-value pair
        // of the ordered dictionary to form the xpath. The key corresponds to the tag name and the value
        // corresponds to the sibling number in the xpath.
        foreach (var orderedDictionary in elementStack)
        {
            var tagName = orderedDictionary.Keys.Cast<string>().Last();
            if (orderedDictionary[tagName] is string count)
            {
                builder.Insert(0, $"/{tagName}[{count}]");
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Browses for file.
    /// </summary>
    /// <param name="startingPath">The starting path.</param>
    /// <param name="filter">The filter.</param>
    /// <returns>Returns the file path. May be an empty string if no file is chosen.</returns>
    internal static string BrowseForFile(string startingPath, string? filter = null)
    {
        using OpenFileDialog openFileDialog = new();
        openFileDialog.InitialDirectory = Path.GetDirectoryName(startingPath);
        if (!string.IsNullOrEmpty(filter))
        {
            openFileDialog.Filter = filter;
        }
        var result = openFileDialog.ShowDialog();
        return result == DialogResult.OK ? openFileDialog.FileName : string.Empty;
    }

    /// <summary>
    /// Shows the confirmation box.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="caption">The caption.</param>
    /// <returns>Returns <c>true</c> if the user confirms, <c>false</c> otherwise.</returns>
    internal static bool ShowConfirmationBox(string message, string caption)
    {
        var button = MessageBoxButtons.YesNo;
        var icon = MessageBoxIcon.Question;
        var result = MessageBox.Show(message, caption, button, icon);
        return result == DialogResult.Yes;
    }

    /// <summary>
    /// Shows the warning box.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="caption">The caption.</param>
    /// <returns></returns>
    internal static void ShowWarningBox(string message, string? caption)
    {
        var button = MessageBoxButtons.OK;
        var icon = MessageBoxIcon.Warning;
        _ = MessageBox.Show(message, caption, button, icon);
    }

}