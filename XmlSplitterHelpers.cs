using BaXmlSplitter;
using BaXmlSplitter.Properties;
using System.Collections;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Xml;
using F23.StringSimilarity;

/// <summary>
/// Collection of static methods and fields that aid the <see cref="XmlSplitter" />.
/// </summary>
internal static class XmlSplitterHelpers
{
    /// <summary>The default output directory.</summary>
    internal const string DEFAULT_OUTPUT_DIR = "WIP";
    /// <summary>The timestamp format.</summary>
    internal const string TIMESTAMP_FORMAT = "HH:mm:ss.fffffff";
    /// <summary>The CSDB programs.</summary>
    internal static readonly string[] PROGRAMS = Enum.GetNames<Programs>();
    /// <summary>The regular expression timeout.</summary>
    internal static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(15);
    /// <summary>The regular expression options.</summary>
    internal const RegexOptions RE_OPTIONS = RegexOptions.Compiled | RegexOptions.Multiline;
    /// <summary>The UOW states file parsing regular expression pattern.</summary>
    internal const string UOW_PATTERN = @"\t*(?:Front Matter: )?(?<tag>\S+)(?: (?<key>\S+))?(?: (?<rs>RS-\d+))?(?: - (?<title>.+?))?(?: (?<lvl>[A-Z0-9 =]+?))? +-- .*?\(state = ""(?<state>[^""]*)""\)$";
    /// <summary>The UOW states file regular expression
    /// object.</summary>
    internal static readonly Regex UOW_REGEX = new(UOW_PATTERN, RE_OPTIONS, TIMEOUT);
    /// <summary>The XML filename pattern.</summary>
    internal const string XML_FILENAME_PATTERN = @"([\w_-]+[\d-]{8,}).*";
    /// <summary>The XML filename regular expression pattern.</summary>
    internal static readonly Regex XML_FILENAME_RE = new(XML_FILENAME_PATTERN, RE_OPTIONS, TIMEOUT);
    /// <summary>The newline strings for Unix and Windows systems.</summary>
    internal static readonly string[] NEWLINES = ["\r\n", "\n"];
    /// <summary>
    /// The xpath separators
    /// </summary>
    internal static readonly string[] XPATH_SEPARATORS = ["|", " or "];
    /// <summary>
    /// The <see cref="Jaccard"/> object for performing string similarity calculations.
    /// </summary>
    internal static readonly Jaccard jaccard = new(k: 2);
    /// <summary>
    /// <para>The priority queue for automatically ordering string similarity results.</para>
    /// <para>This will be used for finding closest matching manual name in the <see cref="XmlSplitter.checkoutItems"/> keys for each manual type in the <see cref="XmlSplitter.manualFromDocnbr"/> dictionary.</para>
    /// </summary>
    internal static readonly PriorityQueue<string, double> bestMatch = new();

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
    /// The CSDB Programs.
    /// </summary>
    internal enum Programs
    {
        /// <summary>
        /// The <c>B_IFM</c> program for instrument flight manuals
        /// </summary>
        B_IFM,
        /// <summary>
        /// The <c>CH604PROD</c> program for Challenger 6XX maintenance manuals
        /// </summary>
        CH604PROD,
        /// <summary>
        /// The <c>CTALPROD</c> program for Challenger 3XX maintenance manuals
        /// </summary>
        CTALPROD,
        /// <summary>
        /// The <c>GXPROD</c> program for Global and Global Express maintenance manuals
        /// </summary>
        GXPROD,
        /// <summary>
        /// The <c>LJ4045PROD</c> program for Learjet 40/45 maintenance manuals
        /// </summary>
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
        long length = fi.Length;
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
        NUL = (char)0 /** Null character */,
        /// <summary>
        /// The backspace character
        /// </summary>
        BS = (char)8 /** Backspace character */,
        /// <summary>
        /// The carriage return character
        /// </summary>
        CR = (char)13 /** Carriage return character */,
        /// <summary>
        /// The substitute character
        /// </summary>
        SUB = (char)26 /** Substitute character */
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
        return (ch > ((int)ControlChars.NUL) && ch < ((int)ControlChars.BS))
            || (ch > ((int)ControlChars.CR) && ch < ((int)ControlChars.SUB));
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
    /// <returns>Returns a Dictionary that maps <see cref="Programs" /> to a Dictionary where the key is the manual name and the value is an array of the element names eligible for checkout.</returns>
    internal static Dictionary<Programs, Dictionary<string, string[]>>? DeserializeCheckoutItems()
    {
        dynamic deserializedCheckoutUowItems = PSSerializer.Deserialize(Resources.CheckoutItems);
        Dictionary<Programs, Dictionary<string, string[]>> __programCheckoutItems = new(PROGRAMS.Length);
        foreach (var program in Enum.GetNames<Programs>())
        {
            ICollection docnbrs = deserializedCheckoutUowItems[program].Keys;
            Dictionary<string, string[]>? checkoutUowItems = new(docnbrs.Count);
            foreach (string docnbr in docnbrs)
            {
                dynamic deserializedUowNames = deserializedCheckoutUowItems[program][docnbr];
                if (deserializedUowNames.ToArray(typeof(string)) is string[] uowNames)
                {
                    checkoutUowItems.Add(docnbr, value: uowNames);
                }
            }
            __programCheckoutItems.Add(Enum.Parse<Programs>(program), checkoutUowItems);
        }
        return __programCheckoutItems;
    }
    /// <summary>
    /// Deserializes the UOW states from PowerShell CLI XML.
    /// </summary>
    /// <returns>Returns a dictionary that maps <see cref="Programs"/> to a Dictionary where the key is the state value (int) and the value is the details of the work state as a <see cref="UowState"/> object.</returns>
    internal static Dictionary<Programs, Dictionary<int, UowState>>? DeserializeStates()
    {
        dynamic deserializedStatesPerProgram = PSSerializer.Deserialize(Resources.StatesPerProgramXml);
        ICollection programs = deserializedStatesPerProgram.Keys;
        Dictionary<Programs, Dictionary<int, UowState>> __statesPerProgram = new(programs.Count);
        foreach (string program in programs.Cast<string>())
        {
            dynamic stateNames = deserializedStatesPerProgram[program];
            Dictionary<int, UowState> states = new(stateNames.Count);
            foreach (int StateValue in stateNames.Keys)
            {
                dynamic stateNameAndRemark = stateNames[StateValue];
                UowState uowState = new(value: StateValue, name: stateNameAndRemark.statename, remark: stateNameAndRemark.remark);
                states.Add(StateValue, uowState);
            }
            __statesPerProgram.Add((Programs)Enum.Parse(typeof(Programs), program), states);
        }
        return __statesPerProgram;
    }
    /// <summary>
    /// Deserializes the Dictionary to get the manual type from the docnbr per program.
    /// </summary>
    /// <returns>Returns a dictionary that maps <see cref="Programs"/> to a Dictionary where the key is the docnbr (string) and the value is the manual type (string).</returns>
    internal static Dictionary<Programs, Dictionary<string, string>>? DeserializeDocnbrManualFromProgram()
    {
        dynamic deserializedDocnbrManualFromProgram = PSSerializer.Deserialize(Resources.DocnbrManualFromProgram);
        ICollection programs = deserializedDocnbrManualFromProgram.Keys;
        Dictionary<Programs, Dictionary<string, string>> __docnbrManualFromProgram = new(programs.Count);
        foreach (string program in programs.Cast<string>())
        {
            dynamic manualFromDocnbr = deserializedDocnbrManualFromProgram[program];
            Dictionary<string, string> __manualFromDocnbr = new(manualFromDocnbr.Count);
            foreach (string docnbr in manualFromDocnbr.Keys)
            {
                dynamic manual = manualFromDocnbr[docnbr];
                __manualFromDocnbr.Add(docnbr, manual);
            }
            __docnbrManualFromProgram.Add((Programs)Enum.Parse(typeof(Programs), program), __manualFromDocnbr);
        }
        return __docnbrManualFromProgram;
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
        if (parent == possibleChild)
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
        if (curNode is XmlDocument doc && doc.ChildNodes is XmlNodeList children && children.Cast<XmlNode>() is IEnumerable<XmlNode> childrenList && childrenList.FirstOrDefault(predicate: child => child.NodeType is not XmlNodeType.Comment and not XmlNodeType.DocumentType) is XmlNode result)
        {
            return result;
        }
        else if /* has key attrib */ (curNode.ParentNode is not null && curNode.ParentNode.Attributes is not null && curNode.ParentNode.Attributes.Cast<XmlAttribute>().Any(attrib => attrib.Name.Equals("key", StringComparison.OrdinalIgnoreCase)))
        {
            return curNode.ParentNode;
        }
        else /* does not have key attrib; need to recurse */
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
        return content.Split(NEWLINES, StringSplitOptions.None)[0];
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
    /// <returns>An array of <see cref="UowState"/> objects corresponding to the states per node indicated by the states file. May be null if parsing fails.</returns>
    internal static UowState[]? ParseUowContent(string? uowContent, string? programStr, Dictionary<Programs, Dictionary<int, UowState>>? statesPerProgram, string? uowStatesFile, out List<LogMessage> logMessages, out Hashtable statesInManual, out string foundDocnbr)
    {
        UowState[] states;
        statesInManual = [];
        logMessages = [];
        if (!string.IsNullOrEmpty(uowContent) && UOW_REGEX.IsMatch(uowContent) && !string.IsNullOrEmpty(programStr) && statesPerProgram is not null && (Programs)Enum.Parse(typeof(Programs), programStr) is Programs program && statesPerProgram[program] is not null)
        {
            foundDocnbr = GetUowStatesDocnbr(ref uowContent);
            MatchCollection stateMatches = UOW_REGEX.Matches(uowContent);
            if (stateMatches.Count == 0)
            {
                logMessages.Add(new LogMessage($"Invalid UOW file '{uowStatesFile}' chosen", Severity.Error));
                return null;
            }
            states = new UowState[stateMatches.Count];
            for (int i = 0; i < stateMatches.Count; i++)
            {
                states[i] = new(tag: stateMatches[i].Groups["tag"].Value);
                string[] xpaths = [$"//{states[i].TagName}", $"//{states[i].TagName?.ToLowerInvariant()}"];

                if (stateMatches[i].Groups["key"].Success && !string.IsNullOrWhiteSpace(stateMatches[i].Groups["key"].Value))
                {
                    states[i].Key = stateMatches[i].Groups["key"].Value;
                    xpaths = xpaths.Select(xpath => $"{xpath}[contains(@key,'{states[i].Key}') or contains(@key,'{states[i].Key?.ToLowerInvariant()}')]").ToArray();
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
                if (stateMatches[i].Groups["state"].Success && !string.IsNullOrWhiteSpace(stateMatches[i].Groups["state"].Value) && int.TryParse(stateMatches[i].Groups["state"].Value, out int stateValue))
                {
                    UowState state = statesPerProgram[program][stateValue];
                    state.StateValue = states[i].StateValue = stateValue;
                    if (!statesInManual.ContainsKey(stateValue))
                    {
                        statesInManual.Add(stateValue, state);
                    }
                    states[i].StateName = state.StateName;
                    states[i].Remark = state.Remark;
                }
                states[i].XPath = string.Join('|', xpaths);
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
    /// Browses for file.
    /// </summary>
    /// <param name="startingPath">The starting path.</param>
    /// <param name="filter">The filter.</param>
    /// <returns>Returns the file path. May be an empty string if no file is chosen.</returns>
    internal static string BrowseForFile(string startingPath, string? filter = null)
    {
        using OpenFileDialog openFileDialog = new()
        {
            InitialDirectory = Path.GetDirectoryName(startingPath)
        };
        if (!string.IsNullOrEmpty(filter))
        {
            openFileDialog.Filter = filter;
        }
        DialogResult result = openFileDialog.ShowDialog();
        if (result == DialogResult.OK)
        {
            return openFileDialog.FileName;
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Shows the confirmation box.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="caption">The caption.</param>
    /// <returns>Returns <c>true</c> if the user confirms, <c>false</c> otherwise.</returns>
    internal static bool ShowConfirmationBox(string message, string caption)
    {
        MessageBoxButtons button = MessageBoxButtons.YesNo;
        MessageBoxIcon icon = MessageBoxIcon.Question;
        DialogResult result = MessageBox.Show(message, caption, button, icon);
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
        MessageBoxButtons button = MessageBoxButtons.OK;
        MessageBoxIcon icon = MessageBoxIcon.Warning;
        _ = MessageBox.Show(message, caption, button, icon);
    }

}