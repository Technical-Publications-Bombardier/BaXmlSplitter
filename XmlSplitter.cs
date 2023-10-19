using BaXmlSplitter.Properties;
using System.Collections;
using System.Drawing.Text;
using System.Management.Automation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
namespace BaXmlSplitter
{
    public partial class XmlSplitter : Form
    {
        [System.Runtime.InteropServices.LibraryImport("gdi32.dll")]
        private static partial IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
            IntPtr pdv, in uint pcFonts);
        private PrivateFontCollection fonts = new();
        private string logFile;
        private string? xmlSourceFile;
        private string? xmlContent;
        private string? uowStatesFile;
        private string? uowContent;
        private string? outputDir;
        private string? xpath;
        private Dictionary<string, Dictionary<int, UowState>>? statesPerProgram;
        private string? Program;
        private const string DEFAULT_OUTPUT_DIR = "WIPItems";
        private static readonly string[] PROGRAMS = new string[] { "B_IFM", "CH604PROD", "CTALPROD", "LJ4045PROD", "GXPROD" };
        private const string TIMESTAMP_FORMAT = "HH:mm:ss.fffffff";
        private record class UowState
        {
            public string? XPath { get; set; }
            public string? TagName { get; set; }
            public string? Key { get; set; }
            public string? Resource { get; set; }
            public string? Title { get; set; }
            public string? Level { get; set; }
            public string? StateName { get; set; }
            public int? StateValue { get; set; }
            public string? Remark { get; set; }
            public UowState(string tagName)
            {
                TagName = tagName;
            }
            public UowState(object? stateName, object? remark)
            {
                StateName = (string)(stateName ?? "");
                Remark = (string)(remark ?? "");
            }

            public override string ToString() => string.Join(", ", GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Where(field => field.GetValue(this) != null)
            .Select(field => $"{field.Name}={field.GetValue(this)}"));
        }
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
        enum Severity
        {
            Hint,
            Warning,
            Error,
            Fatal
        };

        private void LoadFonts()
        {
            uint dummy = 0;
            var seventyTwoFonts = new byte[][] { Resources._72_Black, Resources._72_Bold, Resources._72_BoldItalic, Resources._72_Condensed, Resources._72_CondensedBold, Resources._72_Italic, Resources._72_Light, Resources._72_Monospace_Bd, Resources._72_Monospace_Rg, Resources._72_Regular };
            for(int i = 0; i < seventyTwoFonts.Length; i++)
            {
                IntPtr data = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(seventyTwoFonts[i].Length);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(seventyTwoFonts[i], 0, data, seventyTwoFonts[i].Length);
                    AddFontMemResourceEx(data, (uint)seventyTwoFonts[i].Length, IntPtr.Zero, in dummy);
                    fonts.AddMemoryFont(data, seventyTwoFonts[i].Length);
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.FreeCoTaskMem(data);
                }
            }
        }
        private void DeSerializeStates()
        {
            dynamic deSerializedStatesPerProgram = PSSerializer.Deserialize(Resources.StatesPerProgramXml);
            ICollection Programs = deSerializedStatesPerProgram.Keys;
            statesPerProgram = new(Programs.Count);
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
                    UowState uowState = new(stateNameAndRemark.statename, stateNameAndRemark.remark);
                    states.Add((int)StateValue, uowState);
                }
                statesPerProgram.Add((string)Program, states);
            }
        }
        public XmlSplitter()
        {
            InitializeComponent();
            logFile = Path.GetTempFileName();
            WriteLog($"Start XML splitting tool with log at '{logFile}'");
        }

        private async void XmlSplitter_Load(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                LoadFonts();
                DeSerializeStates();
            });
            WriteLog("Finished initializing.");
        }

        private void WriteLog(string message, Severity severity = Severity.Hint, bool NoNewLine = false)
        {
            using FileStream log = File.Open(logFile, FileMode.Append);
            using StreamWriter logWriter = new(log);
            string timestamped = string.Format("{0}:\t{1}", DateTime.Now.ToString(TIMESTAMP_FORMAT), message + (NoNewLine ? "" : Environment.NewLine));
            logWriter.Write(timestamped);
            logWriter.Flush();
            logWriter.Close();
            logWriter.Dispose();
            logTextBox.Text += timestamped;
            logTextBox.Select(0, logTextBox.Text.Length);
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
            // scroll to the bottom of the logTextBox
            logTextBox.SelectionStart = logTextBox.Text.Length;
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

        private static bool ReadText(out string content, string filename, out Exception? exception)
        {
            try
            {
                content = File.ReadAllText(filename);
                exception = null;
                return true;
            }
            catch (IOException e)
            {
                content = string.Empty;
                exception = e;
                return false;
            }
        }

        private static void ShowWarningBox(string message, string caption)
        {
            MessageBoxButtons button = MessageBoxButtons.OK;
            MessageBoxIcon icon = MessageBoxIcon.Warning;
            MessageBox.Show(message, caption, button, icon);
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
                await Task.Run(() => TextFileChosen(out xmlContent, xmlSelectTextBox.Text, xmlSelectTextBox, "XML") );
                WriteLog("Done reading XML file into memory.");
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
                await Task.Run(()=> TextFileChosen(out uowContent, uowTextBox.Text, uowTextBox, "UOW states") );
                WriteLog("Done reading UOW file into memory.");
            }
        }

        private void OutDirTextBox_TextChanged(object sender, EventArgs e)
        {
            outDirTextBox.Select(outDirTextBox.Text.Length, 0);
            if(outputDir != outDirTextBox.Text)
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
            if (string.IsNullOrEmpty(uowTextBox.Text) || string.IsNullOrEmpty(uowContent))
            {
                ShowWarningBox("Please select a UOW states file before executing the split.", "Cannot proceed: No UOW states provided.");
                WriteLog("Prematurely attempted to begin splitting prior to specifying UOW states", Severity.Warning);
                return;
            }
            if (string.IsNullOrEmpty(Program) || string.IsNullOrEmpty(programsComboBox.Text))
            {
                ShowWarningBox("Please select a program before executing the split.", "Cannot proceed: No program specified.");
                WriteLog("Prematurely attempted to begin splitting prior to specifying program", Severity.Warning);
                return;
            }
            if (xmlSourceFile == null || string.IsNullOrEmpty(xmlContent))
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
            if (string.IsNullOrEmpty(xpath))
            {
                await Task.Run(()=> ProcessUowStates());
            }
            if (xmlContent != null && xpath != null && !string.IsNullOrEmpty(xmlSourceFile))
            {
                execButton.Visible = false;
                // show the progress bar
                ProgressBar progressBar = new()
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
                XmlDocument xml = new();
                xml.LoadXml(xmlContent);
                var nodes = xml.SelectNodes(xpath);
                WriteLog(string.Format("Splitting XML file '{0}' into {1} fragments", Path.GetFileName(xmlSourceFile), nodes?.Count));
                for (int i = 0; i < nodes?.Count; i++)
                {
                    progressBar.Value = (int)(100 * (i + 1) / nodes.Count);
                    XmlDocument xmlFragment = new();
                    if (nodes[i] == null)
                    {
                        continue;
                    }
                    _ = xmlFragment.AppendChild(xmlFragment.ImportNode(nodes[i]!, true));
                    var key = nodes[i]?.Attributes?["key"]?.Value;
                    if (key != null)
                    {
                        var outPath = Path.Combine(outDirTextBox.Text, string.Format("{0}-{1}.xml", Path.GetFileNameWithoutExtension(xmlSourceFile), key));
                        // write the fragment to the outPath
                        xmlFragment.Save(outPath);
                        WriteLog(string.Format("Wrote fragment to '{0}'", outPath), Severity.Hint);
                    }
                    else
                    {
                        WriteLog(string.Format("Unable to get 'key' attribute from node {0}", nodes[i]!.Name), Severity.Hint);
                    }

                }
            }
        }

        private void TextFileChosen(out string content, string path, TextBox textBox, string fileDescription)
        {
            if (ReadText(out content, path, out Exception? exception))
            {
                textBox.Text = path;
                textBox.Select(textBox.Text.Length, 0);
            }
            else
            {
                var message = exception?.Message;
                ShowWarningBox(string.Format("Unable to read {0} file '{1}'\n{2}", fileDescription, path, message), string.Format("{0} file '{1}' unreadable", fileDescription, path));
            }
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
                            if (IsBinary(path) && !ShowConfirmationBox(string.Format("The file at '{0}' appears to be a binary file, not text. Continue?", path), string.Format("{0} appears to be binary", (new FileInfo(path)).Name)))
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

        private void ProcessUowStates()
        {
            if (uowContent != null && !string.IsNullOrEmpty(Program))
            {
                WriteLog("Parsing units of work states file");
                var stateMatches = Regex.Matches(uowContent, @"\t*(?:Front Matter: )?(?<tag>\S+)(?: (?<key>\S+))?(?: (?<rs>RS-\d+))?(?: - (?<title>.+?))?(?: (?<lvl>[A-Z0-9 =]+?))? +-- .*?\(state = ""(?<state>[^""]*)""\)$", RegexOptions.Multiline);
                if(stateMatches.Count == 0)
                {
                    ShowWarningBox("The file chosen as the units of work states does not appear to be valid; please choose another", "Invalid UOW file");
                    WriteLog($"Invalid UOW file '{uowStatesFile}' chosen", Severity.Error);
                    return;
                }
                var states = new UowState[stateMatches.Count];
                Hashtable statesInManual = new();
                for (int i = 0; i < stateMatches.Count; i++)
                {
                    states[i] = new(stateMatches[i].Groups["tag"].Value);
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
                        if (!statesInManual.ContainsKey(stateValue))
                        {
                            statesInManual.Add(stateValue, state);
                        }
                        states[i].StateValue = stateValue;
                        states[i].StateName = state.StateName;
                        states[i].Remark = state.Remark;
                    }
                    states[i].XPath = string.Join('|', xpaths);
                }
                WriteLog(string.Format("Found {0} distinct work states in the manual: {1}", statesInManual.Count, string.Join("; ", statesInManual.Values.Cast<UowState>().Select(uow => uow.ToString()))));
                var listView = new ListView
                {
                    BackColor = SystemColors.Control,
                    BorderStyle = BorderStyle.None,
                    Dock = DockStyle.Fill,
                    FullRowSelect = true,
                    HeaderStyle = ColumnHeaderStyle.Nonclickable,
                    HideSelection = false,
                    Location = new Point(0, 0),
                    MultiSelect = true,
                    Name = "UOWStatesInManualSelectList",
                    Size = this.Size,
                    TabIndex = 0,
                    UseCompatibleStateImageBehavior = false,
                    View = View.Details
                };
                listView.Columns.AddRange(new ColumnHeader[]
                {
                        new ColumnHeader { Name = "Value", Text = "Value", TextAlign = HorizontalAlignment.Right, Width = (int)(Size.Width*0.1111) },
                        new ColumnHeader { Name = "Name", Text = "Name", TextAlign = HorizontalAlignment.Center, Width = (int)(Size.Width*0.2222) },
                        new ColumnHeader { Name = "Remark", Text = "Remark", TextAlign = HorizontalAlignment.Left, Width = (int)(Size.Width*0.6667) }
                });
                var items = statesInManual.Values.Cast<UowState>().Select(state => new ListViewItem(new string[] { state.StateValue.ToString() ?? "", state.StateName ?? "", state.Remark ?? "" }));
                listView.Items.AddRange(items.ToArray());
                // display the multi select list view
                var dialog = new Form
                {
                    Text = "Select UOW states from this manual on which to split",
                    Size = this.Size,
                    StartPosition = FormStartPosition.CenterParent,
                    Icon = Resources.Icon
            };
                dialog.Controls.Add(listView);
                dialog.AcceptButton = new Button { DialogResult = DialogResult.OK, Text = "OK" };
                dialog.CancelButton = new Button { DialogResult = DialogResult.Cancel, Text = "Cancel" };
                dialog.ShowDialog();
                if (dialog.DialogResult == DialogResult.OK)
                {
                    var selectedIndices = listView.SelectedIndices;
                    var chosenStatesToSplit = selectedIndices.Cast<int>().Select(index => (statesInManual[index] as UowState)?.StateValue).ToArray();
                    if (selectedIndices.Count > 0)
                    {
                        xpathTextBox.Text = xpath = string.Join('|', states.Where(state => chosenStatesToSplit.Contains(state.StateValue)).Select(state => state.XPath));
                        xpathGroupBox.Visible = true;
                    }
                    else
                    {
                        ShowWarningBox("Please re-execute the split to select the states.", "No states chosen");
                        WriteLog("No states chosen to split manual.", Severity.Warning);
                    }
                }
            }
        }

        private void ProgramGroupBox(object sender, EventArgs e)
        {
            if(Program != programsComboBox.Text && PROGRAMS.Contains<string>(programsComboBox.Text))
            {
                Program = programsComboBox.Text;
                WriteLog($"Program chosen for manual: {Program}");
            }
        }

    }
}
