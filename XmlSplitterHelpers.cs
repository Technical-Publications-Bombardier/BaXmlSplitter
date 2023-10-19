using BaXmlSplitter;
using BaXmlSplitter.Properties;
using System.Collections;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Serialization;

internal static class XmlSplitterHelpers
{
    internal const string DEFAULT_OUTPUT_DIR = "WIP";
    internal const string TIMESTAMP_FORMAT = "HH:mm:ss.fffffff";
    internal static readonly string[] PROGRAMS = ["B_IFM", "CH604PROD", "CTALPROD", "LJ4045PROD", "GXPROD"];
    internal static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(15);
    internal const RegexOptions RE_OPTIONS = RegexOptions.Compiled | RegexOptions.Multiline;
    internal const string UOW_PATTERN = @"\t*(?:Front Matter: )?(?<tag>\S+)(?: (?<key>\S+))?(?: (?<rs>RS-\d+))?(?: - (?<title>.+?))?(?: (?<lvl>[A-Z0-9 =]+?))? +-- .*?\(state = ""(?<state>[^""]*)""\)$";
    internal const string XML_FILENAME_PATTERN = @"([\w_-]+[\d-]{8,}).*";
    internal static readonly Regex UOW_REGEX = new(UOW_PATTERN, RE_OPTIONS, TIMEOUT);
    internal static readonly Regex XML_FILENAME_RE = new(XML_FILENAME_PATTERN, RE_OPTIONS, TIMEOUT);
    internal static readonly string[] NEWLINES = ["\r\n", "\n"];
    internal static readonly string[] XPATH_SEPARATORS = ["|", " or "];

    #region BinaryCheck
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

    internal enum ControlChars
    {
        NUL = (char)0 /** Null character */,
        BS = (char)8 /** Backspace character */,
        CR = (char)13 /** Carriage return character */,
        SUB = (char)26 /** Substitute character */
    }
    public static bool IsControlChar(int ch)
    {
        return (ch > ((int)ControlChars.NUL) && ch < ((int)ControlChars.BS))
            || (ch > ((int)ControlChars.CR) && ch < ((int)ControlChars.SUB));
    }
    #endregion BinaryCheck
    internal static Dictionary<string, string[]>? DeserializeCheckoutItems()
    {
        dynamic deserializedCheckoutUowItems = PSSerializer.Deserialize(Resources.CheckoutItems);
        ICollection docnbrs = deserializedCheckoutUowItems.Keys;
        Dictionary<string, string[]>? __checkoutUowItems = new(docnbrs.Count);
        foreach (string docnbr in docnbrs)
        {
            dynamic deserializedUowNames = deserializedCheckoutUowItems[docnbr];
            if (deserializedUowNames.ToArray(typeof(string)) is string[] uowNames)
            {
                __checkoutUowItems.Add(docnbr, value: uowNames);
            }
        }
        return __checkoutUowItems;
    }
    internal static Dictionary<string, Dictionary<int, UowState>>? DeserializeStates()
    {
        dynamic deserializedStatesPerProgram = PSSerializer.Deserialize(Resources.StatesPerProgramXml);
        ICollection programs = deserializedStatesPerProgram.Keys;
        Dictionary<string, Dictionary<int, UowState>> __statesPerProgram = new(programs.Count);
        foreach (string program in programs)
        {
            dynamic stateNames = deserializedStatesPerProgram[program];
            if (stateNames == null)
            {
                continue;
            }
            Dictionary<int, UowState> states = new(stateNames.Count);
            foreach (int StateValue in stateNames.Keys)
            {
                dynamic stateNameAndRemark = stateNames[StateValue];
                if (stateNameAndRemark == null)
                {
                    continue;
                }
                UowState uowState = new(value: StateValue, name: stateNameAndRemark.statename, remark: stateNameAndRemark.remark);
                states.Add(StateValue, uowState);
            }
            __statesPerProgram.Add(program, states);
        }
        return __statesPerProgram;
    }
    internal static bool PathIsValid(string pathToTest)
    {
        return !string.IsNullOrWhiteSpace(pathToTest) && Uri.IsWellFormedUriString(string.Format("{0}/{1}", Uri.UriSchemeFile + Uri.SchemeDelimiter, Regex.Replace(pathToTest.Replace(@"\", "/"), @"^//", "")), UriKind.RelativeOrAbsolute);
    }
    internal static XmlNode CalculateParentTag(XmlNode curNode)
    {
        if (curNode is XmlDocument doc && doc.ChildNodes is XmlNodeList children && children.Cast<XmlNode>() is IEnumerable<XmlNode> childrenList && childrenList.FirstOrDefault(predicate: child => child.NodeType is not XmlNodeType.Comment and not XmlNodeType.DocumentType) is XmlNode result)
        {
            return result;
        }
        else if /* has key attrib */ (curNode.ParentNode != null && curNode.ParentNode.Attributes != null && curNode.ParentNode.Attributes.Cast<XmlAttribute>().Any(attrib => attrib.Name.Equals("key", StringComparison.OrdinalIgnoreCase)))
        {
            return curNode.ParentNode;
        }
        else /* does not have key attrib; need to recurse */
        {
            return CalculateParentTag(curNode.ParentNode!);
        }
    }
    internal static string GetUowStatesDocnbr(ref string content)
    {
        return content.Split(NEWLINES, StringSplitOptions.None)[0];
    }
    internal static UowState[]? ParseUowContent(string? uowContent, string? Program, Dictionary<string, Dictionary<int, UowState>>? statesPerProgram, string? uowStatesFile, out List<LogMessage> logMessages, out Hashtable statesInManual, out string foundDocnbr)
    {
        UowState[] states;
        statesInManual = [];
        logMessages = [];
        if (!string.IsNullOrEmpty(uowContent) && UOW_REGEX.IsMatch(uowContent) && !string.IsNullOrEmpty(Program) && statesPerProgram != null && statesPerProgram[Program] != null)
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
                    UowState state = statesPerProgram[Program][stateValue];
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

    internal static bool ShowConfirmationBox(string message, string caption)
    {
        MessageBoxButtons button = MessageBoxButtons.YesNo;
        MessageBoxIcon icon = MessageBoxIcon.Question;
        DialogResult result = MessageBox.Show(message, caption, button, icon);
        return result == DialogResult.Yes;
    }

    internal static void ShowWarningBox(string message, string? caption)
    {
        MessageBoxButtons button = MessageBoxButtons.OK;
        MessageBoxIcon icon = MessageBoxIcon.Warning;
        _ = MessageBox.Show(message, caption, button, icon);
    }

}