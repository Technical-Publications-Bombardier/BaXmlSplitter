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
        internal static partial IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, in uint pcFonts);

        private PrivateFontCollection fonts = new();
        private string logFile;
        private string? splittingReportHtml;
        private XmlSplitReport? report;
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
        private IEnumerable<UowState>? fullyQualifiedSelectedStates;

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

        private async void XmlSplitter_Load(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                LoadFonts();
                statesPerProgram = XmlSplitterHelpers.DeSerializeStates();
            });
            WriteLog("Finished initializing.");
        }

        private void WriteLog(string message, Severity severity = Severity.Hint, bool NoNewLine = false, bool SkipFile = false)
        {
            string timestamped = string.Format("{0}:\t{1}", DateTime.Now.ToString(XmlSplitterHelpers.TIMESTAMP_FORMAT), message + (NoNewLine ? "" : Environment.NewLine));
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
            }
            else
            {
                logDelegate();
            }
        }

        private void BrowseXml(object sender, EventArgs e)
        {
            string filename = XmlSplitterHelpers.BrowseForFile(filter: "Flight or maintenance manual|*.xml", startingPath: xmlSelectTextBox.Text);
            xmlSelectTextBox.Text = filename;
            XmlSelectTextBox_TextChanged(sender, e);
        }

        private void BrowseUow(object sender, EventArgs e)
        {
            string filename = XmlSplitterHelpers.BrowseForFile(filter: "UOW states file|*.*", startingPath: uowTextBox.Text);
            while (Path.GetExtension(filename).Equals(".xml", StringComparison.OrdinalIgnoreCase) && !XmlSplitterHelpers.ShowConfirmationBox($"The chosen UOW states file {Path.GetFileName(filename)} appears to be an XML file, not a file generated by the $LAUNCH tool in Unix. Proceed anyway?", "XML chosen as UOW states file"))
            {
                filename = XmlSplitterHelpers.BrowseForFile(filter: "UOW states file|*.*", startingPath: uowTextBox.Text);
            }
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
                    outDirTextBox.Text = Path.Combine(container, XmlSplitterHelpers.DEFAULT_OUTPUT_DIR);
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
            if (!string.IsNullOrWhiteSpace(uowTextBox.Text) && Uri.IsWellFormedUriString(string.Format("{0}/{1}", Uri.UriSchemeFile + Uri.SchemeDelimiter, Regex.Replace(uowTextBox.Text.Replace(@"\", "/"), @"^//", "")), UriKind.RelativeOrAbsolute) && File.Exists(uowTextBox.Text) && (!XmlSplitterHelpers.IsBinary(uowTextBox.Text) || XmlSplitterHelpers.ShowConfirmationBox(string.Format("The file at '{0}' appears to be a binary file, not text. Continue?", uowTextBox.Text), string.Format("File '{0}' is not text", (new FileInfo(uowTextBox.Text)).Name))))
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
                XmlSplitterHelpers.ShowWarningBox("Please select a UOW states file before executing the split.", "Cannot proceed: No UOW states provided.");
                WriteLog("Prematurely attempted to begin splitting prior to specifying UOW states", Severity.Warning);
                return;
            }
            if (string.IsNullOrEmpty(Program) && string.IsNullOrEmpty(programsComboBox.Text))
            {
                XmlSplitterHelpers.ShowWarningBox("Please select a program before executing the split.", "Cannot proceed: No program specified.");
                WriteLog("Prematurely attempted to begin splitting prior to specifying program", Severity.Warning);
                return;
            }
            if (xmlSourceFile == null && string.IsNullOrEmpty(xmlContent))
            {
                XmlSplitterHelpers.ShowWarningBox("Please select an XML file before executing the split.", "Cannot proceed: No XML file provided.");
                WriteLog("Prematurely attempted to begin splitting prior to specifying XML file", Severity.Warning);
                return;
            }
            if (string.IsNullOrEmpty(outputDir))
            {
                XmlSplitterHelpers.ShowWarningBox("Please select an output directory before executing the split.", "Cannot proceed: No output directory provided.");
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
                    XmlSplitterHelpers.ShowWarningBox(string.Format("Unable to create output directory '{0}': {1}", outputDir, ex.Message), "Cannot proceed: Unable to create output directory.");
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
                            XmlSplitterHelpers.ShowWarningBox(logMessage.Message, Enum.GetName(logMessage.Severity));
                        }
                    }
                }
                if (string.IsNullOrEmpty(xpath) || fullyQualifiedSelectedStates == null)
                {
                    WriteLog("Stopping split. No units of work selected.", Severity.Error);
                }
                else
                {
                    var nodes = await Task.Run(() => xml.SelectNodes(xpath));
                    if (nodes != null && nodes.Count > 0)
                    {
                        WriteLog(string.Format("Splitting XML file '{0}' into {1} fragments", Path.GetFileName(xmlSourceFile), nodes.Count));
                        report = new(capacity: nodes.Count);
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
                                    display: inline-block;
                                    width: 1.3em;
                                    line-height: 0.9em;
                                    color: #2196F3;
                                    text-shadow: 1px 1px 4px black;
                                }
                                pre {
                                    display: block;
                                    background: black;
                                    color: aliceblue;
                                    padding: 0.75em;
                                    min-height: 4vh;
                                }
                            </style>
                        </head>
                        """);
                        /*
                         * Regex uppercaseXPathRe = new(@"//([^a-z\[]+)\[contains\(@key,'([^'a-z]+)'\)", RegexOptions.Compiled);
                         * string[] xpathPartials = xpath.Split(XmlSplitterHelpers.XPATH_SEPARATORS, StringSplitOptions.None).Where(x => x.Contains("//") && uppercaseXPathRe.IsMatch(x)).Select(partial => partial + "]").ToArray();
                         */
                        string dateTimeNow = DateTime.Now.ToString("yyyy - MM - dd - HH - mm - ss - fffffff");
                        string reportBaseFilename = $"{Path.GetFileNameWithoutExtension(xmlSourceFile)}SplittingReport - {dateTimeNow}";
                        string[] splittingReportFilenames = Enum.GetNames<XmlSplitReport.ReportFormat>().Select(format => $"{reportBaseFilename}.{format.ToLowerInvariant()}").ToArray();
                        _ = htmlReportBuilder.Append($"""
                        <body>
                            <p>The source XML, '{Path.GetFileName(xmlSourceFile)}', was split into {nodes.Count} unit of work nodes.</p>
                            <p>The complete splitting report is written as {string.Join(", ", Enum.GetNames<XmlSplitReport.ReportFormat>())} in the WIP package with the following file names:</p>
                            <ul>
                                {string.Join(Environment.NewLine, splittingReportFilenames.Select(filename => $"<li>{filename}</li>").ToArray())}
                            </ul>
                            <p>Below is the full HTML report of the XML splitting results:</p>
                            <table>
                                <caption><p>Table showing the details on each node that was split from the source XML.</p><aside aria-label="Information note">The opening tag of the parent is built from the parent's name and its attribute name-value pairs. It may be slightly different than how it appears in the original XML.</aside><aside aria-label="Information note">Note also that "Node" in this context refers to the unit of work as XML element that was split off from the source XML.</aside></caption>
                                <colgroup><col /><col /><col /><col /><col /><col /><col /><col /><col /></colgroup>
                                <tr>
                                    <th>Node Number</th>
                                    <th>Node Element Name</th>
                                    <th>'Key' Value</th>
                                    <th>Immediate Parent Opening Tag</th>
                                    <th>Full XPath</th>
                                    <th>Filename of Split</th>
                                    <th>UOW State Value</th>
                                    <th>UOW State Name</th>
                                    <th>UOW State Remark</th>
                                </tr>
                        """);
                        if (fullyQualifiedSelectedStates.ToArray() is UowState[] sourceStates)
                        {
                            HashSet<XmlNode> notSeenNodes = new(nodes.Count);
                            int nodeNum = 1;
                            for (int i = 0; i < sourceStates.Length; i++)
                            {
                                progressBar.Value = (int)(100 * (i + 1) / sourceStates.Length);
                                if(sourceStates[i] is UowState curState && curState.XPath is string curXPath && xml.SelectSingleNode(curXPath) is XmlNode curNode && notSeenNodes.Add(curNode))
                                {
                                    BaXmlDocument xmlFragment = new()
                                    {
                                        ResolveEntities = false
                                    };
                                    _ = await Task.Run(() => xmlFragment.AppendChild(xmlFragment.ImportNode(curNode, true)));
                                    if (curNode.Attributes?["key"]?.Value is string key)
                                    {
                                        var outPath = XmlSplitterHelpers.XML_FILENAME_RE.Replace(Path.GetFileNameWithoutExtension(xmlSourceFile), m => Regex.Replace(m.Groups[1].Value, @"[_-]$", string.Empty));
                                        outPath = Path.Combine(outputDir, string.Format("{0}-{1}.xml", outPath, key));
                                        // write the fragment to the outPath
                                        await Task.Run(() => xmlFragment.Save(outPath));
                                        WriteLog(string.Format("Wrote fragment to '{0}'", outPath), Severity.Hint);
                                        XmlNode? parent = curNode.ParentNode;
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
                                        XmlSplitReportEntry reportEntry = new(nodeNumber: nodeNum++, nodeElementName: curNode.Name, keyValue: key, immediateParentOpeningTag: parentTag, fullXPath: GenerateUniqueXPath(curNode), filenameOfSplit: Path.GetFileName(outPath), uowState: curState);
                                        report.Add(reportEntry);
                                        _ = htmlReportBuilder.AppendFormat(@"<tr><!-- Node Number --><td>{0}</td>", reportEntry.NodeNumber);
                                        _ = htmlReportBuilder.AppendFormat(@"<!-- Node Element Name --><td>{0}</td>", reportEntry.NodeElementName);
                                        _ = htmlReportBuilder.AppendFormat(@"<!-- 'Key' Value --><td>{0}</td>", reportEntry.KeyValue);
                                        _ = htmlReportBuilder.AppendFormat(@"<!-- Immediate Parent Opening Tag --><td><code>{0}</code></td>", string.IsNullOrEmpty(reportEntry.ImmediateParentOpeningTag) ? "&nbsp;" : HttpUtility.HtmlEncode(reportEntry.ImmediateParentOpeningTag));
                                        _ = htmlReportBuilder.AppendFormat(@"<!-- Full XPath --><td><code>{0}</code></td>", HttpUtility.HtmlEncode(reportEntry.FullXPath));
                                        _ = htmlReportBuilder.AppendFormat(@"<!-- Filename of Split --><td>{0}</td>", reportEntry.FilenameOfSplit);
                                        _ = htmlReportBuilder.AppendFormat(@"<!-- UOW State Value --><td>{0}</td>", reportEntry.UowState.StateValue);
                                        _ = htmlReportBuilder.AppendFormat(@"<!-- UOW State Name --><td>{0}</td>", reportEntry.UowState.StateName);
                                        _ = htmlReportBuilder.AppendFormat(@"<!-- UOW State Remark --><td>{0}</td></tr>", string.IsNullOrEmpty(reportEntry.UowState.Remark) ? "&nbsp;" : HttpUtility.HtmlEncode(reportEntry.UowState.Remark));
                                    }
                                    else
                                    {
                                        WriteLog(string.Format("Unable to get 'key' attribute from node #{0} ({1}) on UOW #{2}. Skipping.", nodeNum, curNode.Name, i + 1), Severity.Warning);
                                    }
                                }
                            }
                            _ = htmlReportBuilder.Append("</table></body></html>");
                            splittingReportHtml = htmlReportBuilder.ToString();
                            WriteLog("Done splitting XML file. Showing results report.");
                            await Task.Run(() => GenerateOtherReports(reportBaseFilename));
                            // display the report in the default browser
                            await Task.Run(() => DisplayHtmlReport());

                        }
                    }

                }
                progressBar.Visible = false;
                stepsPanel.Controls.Remove(progressBar);
                execButton.Visible = true;
            }
        }

        private void GenerateOtherReports(string baseFilename)
        {
            if (report != null && !string.IsNullOrEmpty(outputDir))
            {
                foreach (XmlSplitReport.ReportFormat format in Enum.GetValuesAsUnderlyingType<XmlSplitReport.ReportFormat>())
                {
                    var reportFilename = Path.Combine(outputDir, $"{baseFilename}.{Enum.GetName(format)}");
                    WriteLog($"Writing report to {reportFilename}");
                    report.Save(reportFilename, format);
                }
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
                if (data.GetData(DataFormats.FileDrop) is string[] paths)
                {
                    foreach (string path in paths.Distinct())
                    {
                        if (File.Exists(path))
                        {
                            if (Path.GetExtension(path).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                            {

                                xmlSelectTextBox.Text = path;
                                XmlSelectTextBox_TextChanged(sender, e);
                            }
                            else
                            {
                                if (XmlSplitterHelpers.IsBinary(path) && !XmlSplitterHelpers.ShowConfirmationBox(string.Format("The file at '{0}' appears to be a binary file, not text. Continue?", path), string.Format("{0} appears to be binary", Path.GetFileName(path))))
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
        }

        private LogMessage[] ProcessUowStates(object sender, EventArgs e)
        {
            List<LogMessage> logMessages = new()
            {
                new LogMessage("Parsing units of work states file")
            };
            UowState[]? states = XmlSplitterHelpers.ParseUowContent(uowContent, Program, statesPerProgram, uowStatesFile, out List<LogMessage> parseUowLogMessages, out Hashtable statesInManual, out string impliedDocnbr);
            if (states == null)
            {
                logMessages.Add(new LogMessage("Not ready to process unit of work states. Check that UOW states file was loaded and parsed properly.", Severity.Warning));
            }
            else
            {
                // check that the root node name is the same as the uowStatesFileDocnbr
                if (xml.DocumentElement == null || string.IsNullOrEmpty(xml.DocumentElement.GetAttribute("docnbr")))
                {
                    logMessages.Add(new LogMessage("No root node docnbr identifiable in XML content. Please check the XML is correct.", Severity.Error));
                    return logMessages.ToArray();
                }
                else if (string.IsNullOrEmpty(impliedDocnbr))
                {
                    logMessages.Add(new LogMessage("The UOW states file was empty. Please check the UOW states file is correct.", Severity.Error));
                    return logMessages.ToArray();
                }
                else if (!xml.DocumentElement.GetAttribute("docnbr").Equals(impliedDocnbr, StringComparison.OrdinalIgnoreCase))
                {
                    logMessages.Add(new LogMessage(string.Format("Root node docnbr '{0}' does not match UOW states file docnbr '{1}'. Please check the UOW states file is correct.", xml.DocumentElement.GetAttribute("docnbr").ToUpperInvariant(), impliedDocnbr.ToUpperInvariant()), Severity.Error));
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
                        fullyQualifiedSelectedStates = states.Where((UowState state) => selectedStates.Select((UowState state) => state.StateValue).Contains(state.StateValue));
                        xpathTextBox.Text = xpath = string.Join('|', fullyQualifiedSelectedStates.Select(state => state.XPath));
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
            if ((string.IsNullOrEmpty(Program) && !string.IsNullOrEmpty(programsComboBox.Text)) || (Program != programsComboBox.Text && XmlSplitterHelpers.PROGRAMS.Contains<string>(programsComboBox.Text)))
            {
                Program = programsComboBox.Text;
                WriteLog($"Program chosen for manual: {Program}");
            }
        }

    }
}
