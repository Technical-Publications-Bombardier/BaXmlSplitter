using CommunityToolkit.Maui.Storage;
using F23.StringSimilarity;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using static MauiXmlSplitter.Models.CsdbContext;

namespace MauiXmlSplitter.Models
{
    public partial class XmlSplitter
    {
        /// <summary>
        /// The path to the source XML file on which the split will be performed.
        /// </summary>
        private string? xmlSourceFile;
        /// <summary>
        /// Gets or sets the XML source file.
        /// </summary>
        /// <value>
        /// The XML source file.
        /// </value>
        public string XmlSourceFile { get => xmlSourceFile ?? string.Empty; set => xmlSourceFile = value; }

        [GeneratedRegex("[_-]$")]
        private static partial Regex TerminusCharPattern();
        /// <summary>The XML filename regular expression pattern.</summary>
        [GeneratedRegex(@"(?<basename>[\w_-]+[\d-]{8,}).*", RegexOptions.Compiled | RegexOptions.Multiline, 15 * 1000)]
        internal static partial Regex XmlFilenameRe();

        /// <summary>
        /// Gets the name of the XML source file base.
        /// </summary>
        /// <value>
        /// The name of the XML source file base.
        /// </value>
        private string XmlSourceFileBaseName => XmlFilenameRe().Replace(Path.GetFileNameWithoutExtension(xmlSourceFile ?? string.Empty), m => TerminusCharPattern().Replace(m.Groups["basename"].Value, string.Empty));

        /// <summary>The string content of the XML file on which the split will be performed. This is 
        /// loaded into memory as soon as the XML is selected.</summary>
        private string? xmlContent;
        /// <summary>The XML object that will parse the XML content. </summary><seealso cref="BaXmlDocument" />
        private readonly BaXmlDocument xml = new()
        {
            ResolveEntities = false
        };
        /// <summary>The path to the file that contains the work states for the units of work (UOW) in the XML.</summary>
        private string? uowStatesFile;
        /// <summary>
        /// Gets or sets path to the file that contains the work states for the units of work (UOW) in the XML.
        /// </summary>
        /// <value>
        /// The path to the file that contains the work states for the units of work (UOW) in the XML.
        /// </value>
        public string UowStatesFile { get => uowStatesFile ?? string.Empty; set => uowStatesFile = value; }
        /// <summary>
        /// The default output directory
        /// </summary>
        const string DefaultOutputDir = "WIP";
        /// <summary>
        /// The content of the UOW states file. This is loaded into memory as soon as the UOW states file is selected.
        /// </summary>
        private string? uowContent;
        /// <summary>
        /// The directory to which the split XML files will be written. <see cref="DefaultOutputDir">By default</see>, this will be the <c>"WIP"</c> directory in the same directory as the source XML file.
        /// </summary>
        private string? outputDirectory;
        /// <summary>
        /// Gets or sets the output directory path.
        /// </summary>
        /// <value>
        /// The output directory path.
        /// </value>
        public string OutputDirectory { get => outputDirectory ?? string.Empty; set => outputDirectory = value; }
        /// <summary>
        /// The XPath string that would be used to select the nodes to split from the XML. This is calculated from the tag names and key values in the UOW states file.
        /// </summary>
        private string? xpath;
        /// <summary>
        /// Gets or sets the XPath.
        /// </summary>
        /// <value>
        /// The XPath that would be used to select the nodes to split from the XML. This is calculated from the tag names and key values in the UOW states file.
        /// </value>
        public string XPath { get => xpath ?? string.Empty; set => xpath = value; }
        /// <summary>A dictionary of all the states in any CSDB program (GXPROD, CTALPROD, B_IFM, CH604PROD, LJ4045PROD).</summary>
        [JsonConverter(typeof(StatesPerProgramConverter))]
        private Dictionary<CsdbProgram, Dictionary<int, UowState>>? statesPerProgram;
        /// <summary>The CSDB element names eligible for importing to RWS Contenta.</summary>
        private Dictionary<CsdbProgram, Dictionary<string, string[]>>? checkoutItems;
        /// <summary>The lookup table for manual type from docnbr.</summary>
        private Dictionary<CsdbProgram, Dictionary<string, string>>? manualFromDocnbr;
        /// <summary>
        /// The <see cref="CsdbProgram"/> lookup from docnbr
        /// </summary>
        private Dictionary<string, CsdbProgram[]>? programPerDocnbr;
        /// <summary>The CSDB program (GXPROD, CTALPROD, B_IFM, CH604PROD, LJ4045PROD)
        /// for the manual.</summary>
        private string? program;
        /// <summary>
        /// Gets or sets the CSDB program (GXPROD, CTALPROD, B_IFM, CH604PROD, LJ4045PROD).
        /// </summary>
        /// <value>
        /// The CSDB program (GXPROD, CTALPROD, B_IFM, CH604PROD, LJ4045PROD).
        /// </value>
        public string Program { get => program ?? string.Empty; set => program = value; }
        /// <summary>The fully populated unit-of-work states that are selected by the user for export.</summary>
        private IEnumerable<UowState>? fullyQualifiedSelectedStates;
        /// <summary>The timestamp format.</summary>
        internal const string LogTimestampFormat = "HH:mm:ss.fffffff";
        /// <summary>The report timestamp format</summary>
        internal const string ReportTimestampFormat = "yyyy - MM - dd - HH - mm - ss - fffffff";
        /// <summary>The CSDB programs.</summary>
        internal static readonly string[] Programs = Enum.GetNames<CsdbProgram>();
        /// <summary>The newline strings for Unix and Windows systems.</summary>
        internal static readonly string[] Newlines = ["\r\n", "\n"];
        /// <summary>
        /// The <see cref="F23.StringSimilarity.Jaccard"/> object for performing string similarity calculations.
        /// </summary>
        internal static readonly Jaccard Jaccard = new(k: 2);
        /// <summary>
        /// <para>The priority queue for automatically ordering string similarity results.</para>
        /// <para>This will be used for finding the closest matching manual name in the <see cref="checkoutItems"/> keys for each manual type in the <see cref="manualFromDocnbr"/> dictionary.</para>
        /// </summary>
        internal static readonly PriorityQueue<string, double> BestMatch = new();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1869:Cache and reuse 'JsonSerializerOptions' instances", Justification = "We deserialize this exactly once on start-up, so it would be overkill to cache the JsonSerializerOptions.")]
        public async Task LoadAssets()
        {
            await using var checkoutItemsStream = await FileSystem.OpenAppPackageFileAsync("CheckoutItems.json").ConfigureAwait(false);
            checkoutItems = await JsonSerializer.DeserializeAsync<Dictionary<CsdbProgram, Dictionary<string, string[]>>>(checkoutItemsStream).ConfigureAwait(false);

            await using var docnbrManualFromProgramStream = await FileSystem.OpenAppPackageFileAsync("DocnbrManualFromProgram.json").ConfigureAwait(false);
            manualFromDocnbr = await JsonSerializer.DeserializeAsync<Dictionary<CsdbProgram, Dictionary<string, string>>>(docnbrManualFromProgramStream).ConfigureAwait(false);

            await using var programPerDocnbrStream = await FileSystem.OpenAppPackageFileAsync("ProgramPerDocnbr.json").ConfigureAwait(false);
            programPerDocnbr = await JsonSerializer.DeserializeAsync<Dictionary<string, CsdbProgram[]>>(programPerDocnbrStream).ConfigureAwait(false);

            await using var statesPerProgramStream = await FileSystem.OpenAppPackageFileAsync("StatesPerProgram.json").ConfigureAwait(false);

            // Create a JsonSerializerOptions object and add the custom converter
            var options = new JsonSerializerOptions();
            options.Converters.Add(new StatesPerProgramConverter());

            // Pass the options to the DeserializeAsync method
            statesPerProgram = await JsonSerializer.DeserializeAsync<Dictionary<CsdbProgram, Dictionary<int, UowState>>>(statesPerProgramStream, options).ConfigureAwait(false);
        }

        /// <summary>
        /// The XML is selected.
        /// </summary>
        /// <returns>Returns <c>true</c> if <see cref="xmlSourceFile" /> and <see cref="xmlContent" /> are both not null or empty, <c>false</c> otherwise.</returns>
        internal bool XmlIsSelected()
        {
            return !string.IsNullOrEmpty(xmlSourceFile) && !string.IsNullOrEmpty(xmlContent);
        }
        /// <summary>
        /// Units of work states for export are selected.
        /// </summary>
        /// <returns>Returns <c>true</c> if <see cref="uowStatesFile" /> and <see cref="uowContent" /> are both not null or empty, <c>false</c> otherwise.</returns>
        internal bool UowIsSelected()
        {
            return !string.IsNullOrEmpty(uowStatesFile) && !string.IsNullOrEmpty(uowContent);
        }
        /// <summary>
        /// Output directory is selected.
        /// </summary>
        /// <returns>Returns <c>true</c> if <see cref="outputDirectory"/> is not null or empty, <c>false</c> otherwise.</returns>
        internal bool OutDirIsSelected()
        {
            return !string.IsNullOrEmpty(outputDirectory);
        }
        /// <summary>
        /// The program for the manual is selected.
        /// </summary>
        /// <returns>Returns <c>true</c> if <see cref="program"/> is not null or empty, <c>false</c> otherwise.</returns>
        internal bool ProgramIsSelected()
        {
            return !string.IsNullOrEmpty(program);
        }
        /// <summary>
        /// <para>
        /// Checks the application is ready perform the manual XML splitting.<para>
        /// </para>If <see cref="XmlIsSelected"/> and <see cref="UowIsSelected"/> and <see cref="OutDirIsSelected"/> and <see cref="ProgramIsSelected"/>, <see cref="execButton"/> is enabled, otherwise it is disabled.
        /// </para>
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private void CheckExecuteSplitIsReady(object sender, EventArgs e)
        {
            throw new NotImplementedException();
            /* TODO: execButton.SuspendLayout();
            if (XmlIsSelected() && UowIsSelected() && OutDirIsSelected() && ProgramIsSelected())
            {
                execButton.Enabled = true;
            }
            else
            {
                execButton.Enabled = false;
            }
            execButton.ResumeLayout(); */
        }
        /// <summary>
        /// Executes the split.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        public async void ExecuteSplit(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(uowContent))
                return;

            if (string.IsNullOrEmpty(program))
                return;

            if (xmlSourceFile is null && string.IsNullOrEmpty(xmlContent))
                return;

            if (string.IsNullOrEmpty(outputDirectory))
                return;

                // check if outputDirectory exists, if not, create it
                if (!Directory.Exists(outputDirectory))
            {
                try
                {
                    _ = Directory.CreateDirectory(outputDirectory);
                }
                catch (IOException ex) when (!Debugger.IsAttached)
                {
                    // await DisplayAlert("Error", "An I/O error occurred while creating the directory.", "OK");
                }
                catch (UnauthorizedAccessException ex) when (!Debugger.IsAttached)
                {
                    // await DisplayAlert("Error", "You do not have permission to create this directory.", "OK");
                }
                catch (ArgumentException ex) when (!Debugger.IsAttached)
                {
                    // await DisplayAlert("Error", "The directory path is invalid.", "OK");
                }
                catch (NotSupportedException ex) when (!Debugger.IsAttached)
                {
                    // await DisplayAlert("Error", "The directory path format is not supported.", "OK");
                }
            }

            if (string.IsNullOrEmpty(xmlContent) || string.IsNullOrEmpty(xmlSourceFile)) return;

        }
    }
}