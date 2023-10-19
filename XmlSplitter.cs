using BaXmlSplitter.Properties;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace BaXmlSplitter
{
    public partial class XmlSplitter : Form
    {
        [System.Runtime.InteropServices.LibraryImport("gdi32.dll")]
        private static partial IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, in uint pcFonts);
        private PrivateFontCollection fonts = new();
        private string logFile;
        private string? splittingReportHtml;
        private string? xmlSourceFile;
        private string? xmlContent;
        private BaXmlDocument xml = new()
        {
            ResolveEntities = false
        };
        private string? uowStatesFile;
        private string? uowContent;
        private string? outputDir;
        private string? xpath;
        private Dictionary<string, Dictionary<int, UowState>>? statesPerProgram;
        private string? Program;
        private const string DEFAULT_OUTPUT_DIR = "WIP";
        private static readonly string[] PROGRAMS = new string[] { "B_IFM", "CH604PROD", "CTALPROD", "LJ4045PROD", "GXPROD" };
        private static readonly TimeSpan TIMEOUT = TimeSpan.FromSeconds(15);
        private const string UOW_PATTERN = @"\t*(?:Front Matter: )?(?<tag>\S+)(?: (?<key>\S+))?(?: (?<rs>RS-\d+))?(?: - (?<title>.+?))?(?: (?<lvl>[A-Z0-9 =]+?))? +-- .*?\(state = ""(?<state>[^""]*)""\)$";
        private static readonly Regex UOW_REGEX = new(UOW_PATTERN, RegexOptions.Compiled | RegexOptions.Multiline, TIMEOUT);
        private const string TIMESTAMP_FORMAT = "HH:mm:ss.fffffff";
        private static readonly string[] NEWLINE = new[] { "\r\n", "\n" };
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

        public static bool IsControlChar(int ch)
        {
            return (ch > ((int)ControlChars.NUL) && ch < ((int)ControlChars.BS))
                || (ch > ((int)ControlChars.CR) && ch < ((int)ControlChars.SUB));
        }

        enum ControlChars
        {
            NUL = (char)0 /** Null character */,
            BS = (char)8 /** Backspace character */,
            CR = (char)13 /** Carriage return character */,
            SUB = (char)26 /** Substitute character */
        }
        private void LoadFonts()
        {
            uint dummy = 0;
            var seventyTwoFonts = new byte[][] { Resources._72_Black, Resources._72_Bold, Resources._72_BoldItalic, Resources._72_Condensed, Resources._72_CondensedBold, Resources._72_Italic, Resources._72_Light, Resources._72_Monospace_Bd, Resources._72_Monospace_Rg, Resources._72_Regular };
            for (int i = 0; i < seventyTwoFonts.Length; i++)
            {
                IntPtr data = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(seventyTwoFonts[i].Length);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(seventyTwoFonts[i], 0, data, seventyTwoFonts[i].Length);
                    _ = AddFontMemResourceEx(data, (uint)seventyTwoFonts[i].Length, IntPtr.Zero, in dummy);
                    fonts.AddMemoryFont(data, seventyTwoFonts[i].Length);
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.FreeCoTaskMem(data);
                }
            }
        }
        public XmlSplitter()
        {
            InitializeComponent();
            logFile = Path.Combine(Path.GetTempPath(), string.Format("BaXmlSplitter-{0}.log", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fffffff")));
            File.Move(Path.GetTempFileName(), logFile);
            WriteLog($"Started XML splitting tool with real time log at '{logFile}'");
        }

        internal static Dictionary<string, Dictionary<int, UowState>>? DeSerializeStates()
        {
            dynamic deSerializedStatesPerProgram = PSSerializer.Deserialize(Resources.StatesPerProgramXml);
            ICollection Programs = deSerializedStatesPerProgram.Keys;
            Dictionary<string, Dictionary<int, UowState>> __statesPerProgram = new(Programs.Count);
            foreach (var Program in Programs)
            {
                dynamic stateNames = deSerializedStatesPerProgram[Program];
                if (stateNames == null)
                {
                    continue;
                }
                var states = new Dictionary<int, UowState>(stateNames.Count);
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
                __statesPerProgram.Add((string)Program, states);
            }
            return __statesPerProgram;
        }

        internal static UowState[]? ParseUowContent(string? uowContent, string? Program, Dictionary<string, Dictionary<int, UowState>>? statesPerProgram, string? uowStatesFile, out List<LogMessage> logMessages, out Hashtable statesInManual, out string foundDoctype)
        {
            UowState[] states;
            statesInManual = new();
            logMessages = new();
            if (!string.IsNullOrEmpty(uowContent) && UOW_REGEX.IsMatch(uowContent) && !string.IsNullOrEmpty(Program) && statesPerProgram != null && statesPerProgram[Program] != null)
            {
                foundDoctype = uowContent.Split(NEWLINE, StringSplitOptions.None)[0];
                var stateMatches = UOW_REGEX.Matches(uowContent);
                if (stateMatches.Count == 0)
                {
                    logMessages.Add(new LogMessage($"Invalid UOW file '{uowStatesFile}' chosen", Severity.Error));
                    return null;
                }
                states = new UowState[stateMatches.Count];
                for (int i = 0; i < stateMatches.Count; i++)
                {
                    states[i] = new(tag: stateMatches[i].Groups["tag"].Value);
                    string[] xpaths = new string[] { $"//{states[i].TagName}", $"//{states[i].TagName?.ToLowerInvariant()}" };

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
                        var state = statesPerProgram[Program][stateValue];
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
                foundDoctype = "";
                return null;
            }
        }
        private async void XmlSplitter_Load(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                LoadFonts();
                statesPerProgram = DeSerializeStates();
            });
            WriteLog("Finished initializing.");
        }

        private void WriteLog(string message, Severity severity = Severity.Hint, bool NoNewLine = false, bool SkipFile = false)
        {
            string timestamped = string.Format("{0}:\t{1}", DateTime.Now.ToString(TIMESTAMP_FORMAT), message + (NoNewLine ? "" : Environment.NewLine));
            if (!SkipFile)
            {
                try
                {
                    using FileStream log = File.Open(logFile, FileMode.Append);
                    using StreamWriter logWriter = new(log);
                    logWriter.Write(timestamped);
                    logWriter.Flush();
                    logWriter.Close();
                    logWriter.Dispose();
                }
                catch (Exception e)
                {
                    WriteLog($"Unable to write to log file at {logFile}: {e.Message}", Severity.Error, false, true);
                }
            }
            void logDelegate()
            {
                logTextBox.SuspendLayout();
                switch (severity)
                {
                    case Severity.Hint:
                        logTextBox.SelectionColor = Color.Lime;
                        break;
                    case Severity.Warning:
                        logTextBox.SelectionColor = Color.Gold;
                        break;
                    case Severity.Error:
                        logTextBox.SelectionColor = Color.OrangeRed;
                        break;
                    case Severity.Fatal:
                        logTextBox.SelectionColor = Color.Red;
                        break;
                }
                logTextBox.AppendText(timestamped);
                logTextBox.ScrollToCaret();
                logTextBox.ResumeLayout();
            }
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(logDelegate);
            } else
            {
                logDelegate();
            }
        }

        private static string BrowseForFile(string? filter = null)
        {
            using OpenFileDialog openFileDialog = new();
            if (!string.IsNullOrEmpty(filter))
            {
                openFileDialog.Filter = filter;
            }
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        private static void ShowWarningBox(string message, string? caption)
        {
            MessageBoxButtons button = MessageBoxButtons.OK;
            MessageBoxIcon icon = MessageBoxIcon.Warning;
            _ = MessageBox.Show(message, caption, button, icon);
        }

        private static bool ShowConfirmationBox(string message, string caption)
        {
            MessageBoxButtons button = MessageBoxButtons.YesNo;
            MessageBoxIcon icon = MessageBoxIcon.Question;
            var result = MessageBox.Show(message, caption, button, icon);
            return result == DialogResult.Yes;
        }

        private void BrowseXml(object sender, EventArgs e)
        {
            string filename = BrowseForFile("Flight or maintenance manual|*.xml");
            xmlSelectTextBox.Text = filename;
            XmlSelectTextBox_TextChanged(sender, e);
        }

        private void BrowseUow(object sender, EventArgs e)
        {
            string filename = BrowseForFile("UOW states file|*.*");
            uowTextBox.Text = filename;
            UowStatesTextBox_TextChanged(sender, e);
        }

        private void BrowseOutDir(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                outDirTextBox.Text = dialog.SelectedPath;
                OutDirTextBox_TextChanged(sender, e);
            }
        }

        private void XPathTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(xpathTextBox.Text))
            {
                xpathTextBox.Visible = true;
            }
            else
            {
                xpathTextBox.Visible = false;
            }
        }
        private async void XmlSelectTextBox_TextChanged(object sender, EventArgs e)
        {
            xmlSelectTextBox.Select(xmlSelectTextBox.Text.Length, 0);
            if (!string.IsNullOrWhiteSpace(xmlSelectTextBox.Text) && Uri.IsWellFormedUriString(string.Format("{0}/{1}", Uri.UriSchemeFile + Uri.SchemeDelimiter, Regex.Replace(xmlSelectTextBox.Text.Replace(@"\", "/"), @"^//", "")), UriKind.RelativeOrAbsolute) && File.Exists(xmlSelectTextBox.Text))
            {
                if (!string.IsNullOrEmpty(xmlSourceFile) && Path.GetFullPath(xmlSourceFile) == Path.GetFullPath(xmlSelectTextBox.Text))
                {
                    return;
                }
                else
                {
                    xmlSourceFile = Path.GetFullPath(xmlSelectTextBox.Text);
                }
                var container = Path.GetDirectoryName(xmlSourceFile);
                if (container != null)
                {
                    outDirTextBox.Text = Path.Combine(container, DEFAULT_OUTPUT_DIR);
                    OutDirTextBox_TextChanged(sender, e);
                }
                WriteLog(string.Format("Reading XML file '{0}'", Path.GetFileName(xmlSourceFile)));
                //TextFileChosen(out xmlContent, xmlSelectTextBox.Text, xmlSelectTextBox, "XML");
                xmlContent = await File.ReadAllTextAsync(xmlSourceFile);
                if (string.IsNullOrEmpty(xmlContent) && new FileInfo(xmlSourceFile).Length > 0)
                {
                    WriteLog("Unable to read XML file. Please check that the file is available and not locked by another process.", Severity.Error);
                }
                else
                {
                    WriteLog("Done reading XML file into memory.");
                }
            }
        }
        private async void UowStatesTextBox_TextChanged(object sender, EventArgs e)
        {
            uowTextBox.Select(uowTextBox.Text.Length, 0);
            if (!string.IsNullOrWhiteSpace(uowTextBox.Text) && Uri.IsWellFormedUriString(string.Format("{0}/{1}", Uri.UriSchemeFile + Uri.SchemeDelimiter, Regex.Replace(uowTextBox.Text.Replace(@"\", "/"), @"^//", "")), UriKind.RelativeOrAbsolute) && File.Exists(uowTextBox.Text) && (!IsBinary(uowTextBox.Text) || ShowConfirmationBox(string.Format("The file at '{0}' appears to be a binary file, not text. Continue?", uowTextBox.Text), string.Format("File '{0}' is not text", (new FileInfo(uowTextBox.Text)).Name))))
            {

                if (!string.IsNullOrEmpty(uowStatesFile) && uowStatesFile == uowTextBox.Text)
                {
                    return;
                }
                uowStatesFile = uowTextBox.Text;
                WriteLog($"Reading UOW file '{Path.GetFileName(uowStatesFile)}'");
                uowContent = await File.ReadAllTextAsync(uowStatesFile);
                if (string.IsNullOrEmpty(uowContent) && new FileInfo(uowStatesFile).Length > 0)
                {
                    WriteLog("Unable to read UOW states file. Please check that the file is available and not locked by another process.", Severity.Error);
                }
                else
                {
                    WriteLog("Done reading UOW file into memory.");
                }
            }
        }

        private void OutDirTextBox_TextChanged(object sender, EventArgs e)
        {
            outDirTextBox.Select(outDirTextBox.Text.Length, 0);
            if (outputDir != outDirTextBox.Text)
            {
                // if the out dir does not exist, set outDirWillBeCreated.Visible to true
                if (!string.IsNullOrWhiteSpace(outDirTextBox.Text) && Uri.IsWellFormedUriString(string.Format("{0}/{1}", Uri.UriSchemeFile + Uri.SchemeDelimiter, Regex.Replace(outDirTextBox.Text.Replace(@"\", "/"), @"^//", "")), UriKind.RelativeOrAbsolute))
                {
                    outputDir = outDirTextBox.Text;
                    if (!Directory.Exists(outputDir))
                    {
                        outDirWillBeCreated.Enabled = outDirWillBeCreated.Visible = true;
                        WriteLog("Output directory will be created.");
                    }
                    else
                    {
                        outDirWillBeCreated.Enabled = outDirWillBeCreated.Visible = false;
                    }
                    WriteLog($"Selected units of work will be written to {outputDir}");
                }
            }
        }

        private async void ExecuteSplit(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(uowTextBox.Text) && string.IsNullOrEmpty(uowContent))
            {
                ShowWarningBox("Please select a UOW states file before executing the split.", "Cannot proceed: No UOW states provided.");
                WriteLog("Prematurely attempted to begin splitting prior to specifying UOW states", Severity.Warning);
                return;
            }
            if (string.IsNullOrEmpty(Program) && string.IsNullOrEmpty(programsComboBox.Text))
            {
                ShowWarningBox("Please select a program before executing the split.", "Cannot proceed: No program specified.");
                WriteLog("Prematurely attempted to begin splitting prior to specifying program", Severity.Warning);
                return;
            }
            if (xmlSourceFile == null && string.IsNullOrEmpty(xmlContent))
            {
                ShowWarningBox("Please select an XML file before executing the split.", "Cannot proceed: No XML file provided.");
                WriteLog("Prematurely attempted to begin splitting prior to specifying XML file", Severity.Warning);
                return;
            }
            if (string.IsNullOrEmpty(outputDir))
            {
                ShowWarningBox("Please select an output directory before executing the split.", "Cannot proceed: No output directory provided.");
                WriteLog("Prematurely attempted to begin splitting prior to specifying output directory", Severity.Warning);
                return;
            }
            // check if outputDir exists, if not, create it
            if (!Directory.Exists(outputDir))
            {
                try
                {
                    _ = Directory.CreateDirectory(outputDir);
                }
                catch (Exception ex)
                {
                    ShowWarningBox(string.Format("Unable to create output directory '{0}': {1}", outputDir, ex.Message), "Cannot proceed: Unable to create output directory.");
                    WriteLog(string.Format("Unable to create output directory '{0}': {1}", outputDir, ex.Message), Severity.Fatal);
                    return;
                }
            }
            if (!string.IsNullOrEmpty(xmlContent) && !string.IsNullOrEmpty(xmlSourceFile))
            {
                execButton.Visible = false;
                // show the progress bar
                using ProgressBar progressBar = new()
                {
                    Location = execButton.Location,
                    Size = execButton.Size,
                    Dock = execButton.Dock,
                    Anchor = execButton.Anchor,
                    TabIndex = execButton.TabIndex,
                    TabStop = execButton.TabStop,
                    Enabled = true,
                    Visible = true,
                    Minimum = 0,
                    Maximum = 100,
                    Value = 0
                };
                stepsPanel.Controls.Add(progressBar);

                await Task.Run(() => xml.LoadXml(xmlContent));

                if (string.IsNullOrEmpty(xpath))
                {
                    var logMessages = await Task.Run(() => ProcessUowStates(sender, e));
                    foreach (LogMessage logMessage in logMessages)
                    {
                        WriteLog(logMessage.Message, logMessage.Severity);
                        if (logMessage.Severity > Severity.Hint)
                        {
                            ShowWarningBox(logMessage.Message, Enum.GetName(logMessage.Severity));
                        }
                    }
                }
                if (string.IsNullOrEmpty(xpath))
                {
                    WriteLog("Stopping split. No XPath.", Severity.Error);
                }
                else
                {
                    var nodes = await Task.Run(() => xml.SelectNodes(xpath));
                    WriteLog(string.Format("Splitting XML file '{0}' into {1} fragments", Path.GetFileName(xmlSourceFile), nodes?.Count));
                    StringBuilder htmlReportBuilder = new();
                    _ = htmlReportBuilder.Append($"""
                        <!DOCTYPE html>
                        <html lang="en">
                        <head>
                            <meta charset="UTF-8">
                            <meta name="viewport" content="width=device-width, initial-scale=1.0">
                            <title>Report on Splitting {Path.GetFileNameWithoutExtension(xmlSourceFile)}</title>
                            <style>
                        """);
                    _ = htmlReportBuilder.Append("""
                                table,
                                th,
                                td {
                                    border: 1px solid black;
                                    border-collapse: collapse;
                                }

                                th,
                                td {
                                    padding: 5px;
                                    text-align: left;
                                }

                                tr:nth-child(even) {
                                    background-color: #eee;
                                }

                                tr:nth-child(odd) {
                                    background-color: #fff;
                                }
                                aside {
                                    background-color: #e7f3fe;
                                    border-left: 6px solid #2196F3;
                                    text-align: left;
                                    padding: 4px;
                                    -webkit-box-shadow: 2px 2px 4px -1px rgba(0, 0, 0, 0.75);
                                    -moz-box-shadow: 2px 2px 4px -1px rgba(0, 0, 0, 0.75);
                                    box-shadow: 2px 2px 4px -1px rgba(0, 0, 0, 0.75);
                                    margin-bottom: 0.5em;
                                }
                                aside::before {
                                    content: "ⓘ";
                                    font-weight: bold;
                                    font-size: 1.5em;
                                    width: 1.3em;
                                    position: relative;
                                    float: left;
                                    color: #2196F3;
                                }
                            </style>
                        </head>
                        """);
                    _ = htmlReportBuilder.Append($"""
                        <body>
                            <p>The source XML, '{Path.GetFileName(xmlSourceFile)}', was split into {nodes?.Count} units of work using the XPath query below:</p>
                            <pre><code>{xpath}</code></pre>
                            <p> This is the full report of the XML splitting results:</p>
                            <table>
                                <caption><p>Table showing the details on each node that was split from the source XML.</p><aside>The opening tag of the parent is built from the parent's name and its attribute name-value pairs. It may be slightly different than how it appears in the original XML.</aside><aside>Note also that "Node" in this context refers to the unit of work as XML element that was split off from the source XML.</aside></caption>
                                <colgroup><col /><col /><col /><col /><col /><col /></colgroup>
                                <tr>
                                    <th>Node Number</th>
                                    <th>Node Element Name</th>
                                    <th>'Key' Value</th>
                                    <th>Immediate Parent Opening Tag</th>
                                    <th>Full XPath</th>
                                    <th>Filename of Split</th>
                                </tr>
                        """);

                    for (int i = 0; i < nodes?.Count; i++)
                    {
                        progressBar.Value = (int)(100 * (i + 1) / nodes.Count);
                        BaXmlDocument xmlFragment = new()
                        {
                            ResolveEntities = false
                        };
                        if (nodes[i] == null)
                        {
                            continue;
                        }

                        _ = await Task.Run(() => xmlFragment.AppendChild(xmlFragment.ImportNode(nodes[i]!, true)));
                        var key = nodes[i]?.Attributes?["key"]?.Value;
                        if (!string.IsNullOrEmpty(key))
                        {
                            var outPath = Path.Combine(outDirTextBox.Text, string.Format("{0}-{1}.xml", Path.GetFileNameWithoutExtension(xmlSourceFile), key));
                            // write the fragment to the outPath
                            await Task.Run(() => xmlFragment.Save(outPath));
                            WriteLog(string.Format("Wrote fragment to '{0}'", outPath), Severity.Hint);
                            _ = htmlReportBuilder.AppendFormat(@"<tr><!-- Node Number --><td>{0}</td>", i + 1);
                            _ = htmlReportBuilder.AppendFormat(@"<!-- Node Element Name --><td>{0}</td>", nodes[i]!.Name);
                            _ = htmlReportBuilder.AppendFormat(@"<!-- 'Key' Value --><td>{0}</td>", key);
                            XmlNode? parent = nodes[i]!.ParentNode;
                            string parentTag = String.Empty;
                            if (parent != null && parent.Attributes != null)
                            {
                                parentTag = $"<{parent.Name}";
                                foreach (XmlAttribute attrib in parent.Attributes)
                                {
                                    parentTag += $" {attrib.Name}=\"{attrib.Value}\"";
                                }
                                parentTag += ">";
                            }
                            else if (parent != null)
                            {
                                parentTag = $"<{parent.Name}>";
                            }
                            _ = htmlReportBuilder.AppendFormat(@"<!-- Immediate Parent Opening Tag --><td><code>{0}</code></td>", string.IsNullOrEmpty(parentTag) ? "&nbsp;" : HttpUtility.HtmlEncode(parentTag));
                            _ = htmlReportBuilder.AppendFormat(@"<!-- Full XPath --><td><code>{0}</code></td>", HttpUtility.HtmlEncode(GenerateUniqueXPath(nodes[i]!)));
                            _ = htmlReportBuilder.AppendFormat(@"<!-- Filename of Split --><td>{0}</td></tr>", Path.GetFileName(outPath));
                        }
                        else
                        {
                            WriteLog(string.Format("Unable to get 'key' attribute from node {0}", nodes[i]!.Name), Severity.Warning);
                        }
                    }
                    _ = htmlReportBuilder.Append("</table></body></html>");
                    splittingReportHtml = htmlReportBuilder.ToString();
                    // display the report in the default browser
                    await Task.Run(() => DisplayHtmlReport());
                }
                progressBar.Visible = false;
                stepsPanel.Controls.Remove(progressBar);
                execButton.Visible = true;
            }
        }

        private void DisplayHtmlReport()
        {
            if (string.IsNullOrEmpty(splittingReportHtml) || string.IsNullOrEmpty(xmlSourceFile))
            {
                WriteLog("No HTML report to display.", Severity.Warning);
                return;
            }
            string reportOutPath = Path.Combine(outDirTextBox.Text, $"Splitting{Path.GetFileNameWithoutExtension(xmlSourceFile)}Report-{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fffffff}.html");
            WriteLog("Displaying HTML report using default browser.");
            try
            {
                File.WriteAllText(reportOutPath, splittingReportHtml);
                ProcessStartInfo psi = new()
                {
                    UseShellExecute = true,
                    FileName = reportOutPath
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString(), Severity.Error);
                return;
            }
            WriteLog($"Wrote XML splitting report to {reportOutPath}");
        }

        private string GenerateUniqueXPath(XmlNode xmlNode)
        {
            if (xmlNode.NodeType == XmlNodeType.Attribute && xmlNode is XmlAttribute xmlNodeAsAttrib && xmlNodeAsAttrib.OwnerElement != null)
            {
                // attributes have an OwnerElement, not a ParentNode; also they have             
                // to be matched by name, not found by position
                return string.Format("{0}/@{1}", GenerateUniqueXPath(xmlNodeAsAttrib.OwnerElement), xmlNode.Name);
            }
            if (xmlNode.ParentNode == null)
            {
                // the only node with no parent is the root node, which has no path
                return string.Empty;
            }

            // Get the Index
            int indexInParent = 1;
            XmlNode? siblingNode = xmlNode.PreviousSibling;
            // Loop thru all Siblings
            while (siblingNode != null)
            {
                // Increase the Index if the Sibling has the same Name
                if (siblingNode.Name == xmlNode.Name)
                {
                    indexInParent++;
                }
                siblingNode = siblingNode.PreviousSibling;
            }

            // the path to a node is the path to its parent, plus "/node()[n]", where n is its position among its siblings.         
            return string.Format("{0}/{1}[{2}]", GenerateUniqueXPath(xmlNode.ParentNode), xmlNode.Name, indexInParent);
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            var data = e.Data;
            if (data != null && data.GetData(DataFormats.FileDrop) != null)
            {
                string[] paths = (string[])(data.GetData(DataFormats.FileDrop) ?? Array.Empty<string>());
                foreach (string path in paths.Distinct())
                {
                    if (File.Exists(path))
                    {
                        if (Regex.IsMatch(path, ".*\\.xml$", RegexOptions.IgnoreCase))
                        {

                            xmlSelectTextBox.Text = path;
                            XmlSelectTextBox_TextChanged(sender, e);
                        }
                        else
                        {
                            if (IsBinary(path) && !ShowConfirmationBox(string.Format("The file at '{0}' appears to be a binary file, not text. Continue?", path), string.Format("{0} appears to be binary", Path.GetFileName(path))))
                            {
                                continue;
                            }
                            uowSelectBox.Text = path;
                            UowStatesTextBox_TextChanged(sender, e);
                        }
                    }
                    else if (Directory.Exists(path))
                    {
                        outDirTextBox.Text = path;
                        OutDirTextBox_TextChanged(sender, e);
                    }
                }
            }
        }

        private LogMessage[] ProcessUowStates(object sender, EventArgs e)
        {
            List<LogMessage> logMessages = new()
            {
                new LogMessage("Parsing units of work states file")
            };
            UowState[]? states = ParseUowContent(uowContent, Program, statesPerProgram, uowStatesFile, out List<LogMessage> parseUowLogMessages, out Hashtable statesInManual, out string impliedDoctype);
            if (states == null)
            {
                logMessages.Add(new LogMessage("Not ready to process unit of work states. Check that UOW states file was loaded and parsed properly.", Severity.Warning));
            }
            else
            {
                // check that the root node name is the same as the uowStatesFileDoctype
                if (xml.DocumentElement == null || string.IsNullOrEmpty(xml.DocumentElement.Name))
                {
                    logMessages.Add(new LogMessage("No root node name identifiable in XML content. Please check the XML is correct.", Severity.Error));
                    return logMessages.ToArray();
                }
                else if (string.IsNullOrEmpty(impliedDoctype))
                {
                    logMessages.Add(new LogMessage("The UOW states file was empty. Please check the UOW states file is correct.", Severity.Error));
                    return logMessages.ToArray();
                }
                else if (!xml.DocumentElement.Name.Equals(impliedDoctype, StringComparison.InvariantCultureIgnoreCase))
                {
                    logMessages.Add(new LogMessage(string.Format("Root node name '{0}' does not match UOW states file doctype '{1}'. Please check the UOW states file is correct.", xml.DocumentElement.Name.ToUpperInvariant(), impliedDoctype.ToUpperInvariant()), Severity.Error));
                    return logMessages.ToArray();
                }
                logMessages.Add(new LogMessage(string.Format("Found {0} distinct work states in the manual:\n\t{1}", statesInManual.Count, string.Join("\n\t", statesInManual.Values.Cast<UowState>().Select(uow => uow.ToString())))));
                var items = statesInManual.Values.Cast<UowState>().ToArray().Select(state => new ListViewItem(new string[] { state.StateValue.ToString() ?? "", state.StateName ?? "", state.Remark ?? "" }));
                // display the multi select list view
                var dialog = new SelectStates(items.ToArray(), states)
                {
                    Font = Font,
                    StartPosition = FormStartPosition.CenterParent,
                    Icon = Resources.Icon
                };
                _ = dialog.ShowDialog();
                if (dialog.DialogResult == DialogResult.OK)
                {
                    var selectedStates = dialog.SelectedStates;
                    if (selectedStates != null && selectedStates.Length > 0)
                    {
                        xpathTextBox.Text = xpath = string.Join('|', states.Where((UowState state) => selectedStates.Select((UowState state) => state.StateValue).Contains(state.StateValue)).Select(state => state.XPath));
                        XPathTextBox_TextChanged(sender, e);
                    }
                    else
                    {
                        logMessages.Add(new LogMessage("No states chosen to split manual.", Severity.Warning));
                    }
                }
            }
            return logMessages.ToArray<LogMessage>();
        }


        private void ProgramGroupBox(object sender, EventArgs e)
        {
            if ((string.IsNullOrEmpty(Program) && !string.IsNullOrEmpty(programsComboBox.Text)) || (Program != programsComboBox.Text && PROGRAMS.Contains<string>(programsComboBox.Text)))
            {
                Program = programsComboBox.Text;
                WriteLog($"Program chosen for manual: {Program}");
            }
        }

    }
}
