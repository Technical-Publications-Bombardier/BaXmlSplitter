using BaXmlSplitter;
using BaXmlSplitter.Properties;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Xml;

internal static class XmlSplitterHelpers
{
    internal const string DEFAULT_OUTPUT_DIR = "WIP";
    internal const string TIMESTAMP_FORMAT = "HH:mm:ss.fffffff";
    internal static readonly string[] PROGRAMS = Enum.GetNames<Programs>();
    internal static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(15);
    internal const RegexOptions RE_OPTIONS = RegexOptions.Compiled | RegexOptions.Multiline;
    internal const string UOW_PATTERN = @"\t*(?:Front Matter: )?(?<tag>\S+)(?: (?<key>\S+))?(?: (?<rs>RS-\d+))?(?: - (?<title>.+?))?(?: (?<lvl>[A-Z0-9 =]+?))? +-- .*?\(state = ""(?<state>[^""]*)""\)$";
    internal const string XML_FILENAME_PATTERN = @"([\w_-]+[\d-]{8,}).*";
    internal static readonly Regex UOW_REGEX = new(UOW_PATTERN, RE_OPTIONS, TIMEOUT);
    internal static readonly Regex XML_FILENAME_RE = new(XML_FILENAME_PATTERN, RE_OPTIONS, TIMEOUT);
    internal static readonly string[] NEWLINES = ["\r\n", "\n"];
    internal static readonly string[] XPATH_SEPARATORS = ["|", " or "];

    internal enum FontFamilies
    {
        _72,
        _72_Black,
        _72_Condensed,
        _72_Light,
        _72_Monospace
    };
    internal enum Programs
    {
        B_IFM,
        CH604PROD,
        CTALPROD,
        GXPROD,
        LJ4045PROD
    };

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
    internal static Dictionary<Programs, Dictionary<string, string>>? DeserializeDocnbrsPerCheckoutItem()
    {
        dynamic deserializedDocnbrsPerCheckoutItem = PSSerializer.Deserialize(Resources.LookupDocnbr);
        Dictionary<Programs, Dictionary<string, string>> __lookupCheckoutPerDocnbrPerProgram = new(PROGRAMS.Length);
        foreach (string program in Enum.GetNames<Programs>())
        {
            ICollection docnbrs = deserializedDocnbrsPerCheckoutItem[program].Keys;
            Dictionary<string, string>? docnbrsPerCheckoutItem = new(docnbrs.Count);
            foreach (string docnbr in docnbrs)
            {
                dynamic deserializedCheckoutItem = deserializedDocnbrsPerCheckoutItem[program][docnbr];
                if (deserializedCheckoutItem is string checkoutItem)
                {
                    docnbrsPerCheckoutItem.Add(docnbr, checkoutItem);
                }
            }
            __lookupCheckoutPerDocnbrPerProgram.Add(Enum.Parse<Programs>(program), docnbrsPerCheckoutItem);
        }
        return __lookupCheckoutPerDocnbrPerProgram;
    }
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
    internal static UowState[]? ParseUowContent(string? uowContent, string? programStr, Dictionary<Programs, Dictionary<int, UowState>>? statesPerProgram, string? uowStatesFile, out List<LogMessage> logMessages, out Hashtable statesInManual, out string foundDocnbr)
    {
        UowState[] states;
        statesInManual = [];
        logMessages = [];
        if (!string.IsNullOrEmpty(uowContent) && UOW_REGEX.IsMatch(uowContent) && !string.IsNullOrEmpty(programStr) && statesPerProgram != null && (Programs)Enum.Parse(typeof(Programs), programStr) is Programs program && statesPerProgram[program] != null)
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