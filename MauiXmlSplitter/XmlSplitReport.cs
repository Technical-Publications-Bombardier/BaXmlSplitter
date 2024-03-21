using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Xml;
using MauiXmlSplitter.Models;

namespace MauiXmlSplitter;

/// <summary>
///     The XML splitting result report in XML, CSV, and TSV.
/// </summary>
public class XmlSplitReport(SynchronizationContext synchronizationContext, string? baseName = null)
    : ConcurrentBag<XmlSplitReportEntry>, IXmlSplitReport<XmlSplitter>
{
    /// <summary>
    ///     Available report formats
    /// </summary>
    public enum ReportFormat
    {
        /// <summary>
        ///     Comma-separated values
        /// </summary>
        Csv,

        /// <summary>
        ///     Tab-separated values
        /// </summary>
        Tsv,

        /// <summary>
        ///     Extensible markup language
        /// </summary>
        Xml
    }


    /// <summary>
    ///     The fields
    /// </summary>
    private static readonly string[] Fields = typeof(XmlSplitReportEntry)
        .GetFields(BindingFlags.Public | BindingFlags.Instance).Select(field => field.Name).ToArray();

    /// <summary>
    ///     Gets the parent tag names.
    /// </summary>
    /// <value>
    ///     The parent tag names.
    /// </value>
    private string[] ParentTagNames => this
        .SelectMany(entry => new[] { entry.CheckoutParent.Name, entry.KeyedParent.Name }).Distinct().ToArray();

    /// <summary>
    ///     Gets or sets the additional attribute names per element.
    /// </summary>
    /// <value>
    ///     The additional attribute names per element.
    /// </value>
    private Dictionary<string, HashSet<string>> AdditionalAttributeNamesPerElement
    {
        get
        {
            Dictionary<string, HashSet<string>> builder = [];
            foreach (var elementName in ParentTagNames)
                builder.Add(elementName, [
                    .. this.Where(entry => entry.KeyedParent.Name == elementName).SelectMany(entry =>
                        entry.KeyedParent.Attributes!.Cast<XmlAttribute>().Select(attribute => attribute.Name)),
                    .. this.Where(entry => entry.CheckoutParent.Name == elementName).SelectMany(entry =>
                        entry.CheckoutParent.Attributes!.Cast<XmlAttribute>().Select(attribute => attribute.Name))
                ]);
            return builder;
        }
    }

    /// <summary>
    ///     Gets the timestamp component of the filename.
    /// </summary>
    /// <value>
    ///     The name.
    /// </value>
    public string Name => BaseName +
                          DateTime.Now.ToString(IXmlSplitReport.ReportTimestampFormat, CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public string BaseName
    {
        get => baseName ?? string.Empty;
        set => baseName = value;
    }

    /// <summary>
    ///     Gets the node number.
    /// </summary>
    /// <value>
    ///     The node number.
    /// </value>
    public int NodeNumber { get; set; }

    /// <summary>
    ///     Adds the <see cref="XmlSplitReportEntry" /> entry.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="checkoutParent">The checkout parent.</param>
    /// <param name="checkoutParentNumber">The sequence number of the checkout parent.</param>
    /// <param name="uowState">State of the uow.</param>
    public void AddEntry(XmlNode node, XmlNode checkoutParent, int checkoutParentNumber, UowState uowState)
    {
        var key = XmlSplitter.GetFragmentName(checkoutParent);
        var fragmentName = !string.IsNullOrEmpty(BaseName) ? $"{BaseName}-{key}.xml" : string.Empty;
        Add(new XmlSplitReportEntry(checkoutParent, checkoutParentNumber, ++NodeNumber, node,
            CalculateParentTag(node), IXmlSplitReport.GenerateUniqueXPath(node), fragmentName, uowState));
        synchronizationContext.Post(_ => OnEntryAdded?.Invoke(), null);
    }

    /// <summary>
    ///     Adds report entries by enumeration.
    /// </summary>
    /// <param name="children">The checkout node's children.</param>
    /// <param name="checkoutParent">The checkout parent node.</param>
    /// <param name="checkoutParentNumber">The checkout parent number.</param>
    /// <returns></returns>
    public void AddEntries(IEnumerable<StateWithNode> children, XmlNode checkoutParent, int checkoutParentNumber)
    {
        foreach (var stateWithNode in children)
            AddEntry(stateWithNode.Child, checkoutParent, checkoutParentNumber, stateWithNode.State);
    }


    /// <inheritdoc />
    public int GetNumSiblings(XmlSplitReportEntry entry)
    {
        return this.Count(e => e.CheckoutParentNumber == entry.CheckoutParentNumber);
    }

    /// <inheritdoc />
    public event Action? OnEntryAdded;


    /// <summary>
    ///     Calculates the parent tag.
    /// </summary>
    /// <param name="curNode">The current node.</param>
    /// <returns></returns>
    internal static XmlNode CalculateParentTag(XmlNode curNode)
    {
        if (curNode is XmlDocument { ChildNodes: { } children } && children.Cast<XmlNode>() is { } childrenList &&
            childrenList.FirstOrDefault(child =>
                child.NodeType is not XmlNodeType.Comment and not XmlNodeType.DocumentType) is { } result)
            return result;
        if /* has key attribute */ (curNode.ParentNode?.Attributes != null && curNode.ParentNode.Attributes
                                        .Cast<XmlAttribute>().Any(attribute =>
                                            attribute.Name.Equals("key", StringComparison.OrdinalIgnoreCase)))
            return curNode.ParentNode;
        /* does not have key attribute; need to recurse */
        return CalculateParentTag(curNode.ParentNode!);
    }

    public async Task SaveAsync(string outPath, ReportFormat outFormat)
    {
        switch (outFormat)
        {
            case ReportFormat.Csv:
            case ReportFormat.Tsv:
            {
                await WriteCsvReportAsync();
                break;
            }
            case ReportFormat.Xml:
            {
                await Task.Run(WriteXmlReport);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(outFormat), outFormat, null);
        }

        return;

        async Task WriteCsvReportAsync()
        {
            await using StreamWriter writer = new(outPath);
            var separator = outFormat == ReportFormat.Csv ? ',' : '\t';
            await writer.WriteLineAsync(string.Join(separator, Fields)).ConfigureAwait(false);
            foreach (var line in this.Select(entry => string.Join(separator,
                     [
                         entry.CheckoutParentNumber.ToString(), entry.CheckoutParent.Name,
                         entry.CheckoutParent.Attributes?["key"]?.Value ?? "&nbsp;", entry.CheckoutParent.OuterXml,
                         entry.NodeNumber.ToString(), entry.UowNode.Name,
                         entry.UowNode.Attributes?["key"]?.Value ?? "&nbsp;", entry.KeyedParent.OuterXml,
                         entry.FullXPath, entry.FilenameOfSplit
                     ]))) await writer.WriteLineAsync(line).ConfigureAwait(false);
        }

        void WriteXmlReport()
        {
            XmlDocument xmlReport = new();
            var root = xmlReport.CreateElement("XmlSplitReport");
            var dtd = """
                      <!ELEMENT XmlSplitReport (Entry*)>
                      <!ELEMENT Entry (CheckoutName, NodeName, Ancestors, XPathToUow, Filename, State)>
                      <!ATTLIST Entry
                                CheckoutNumber CDATA #REQUIRED
                                NodeNumber CDATA #REQUIRED
                                CheckoutKey CDATA #REQUIRED
                                UowKey CDATA #REQUIRED>
                      <!ELEMENT CheckoutName (#PCDATA)>
                      <!ELEMENT NodeName (#PCDATA)>
                      """;
            dtd += Environment.NewLine;
            dtd +=
                $"<!ELEMENT Ancestors ({(ParentTagNames.Length > 0 ? string.Join('|', ParentTagNames) : "EMPTY")})*>";
            dtd += Environment.NewLine;
            dtd += AdditionalAttributeNamesPerElement.Count > 0
                ? string.Join(Environment.NewLine, ParentTagNames.Select(name =>
                {
                    string[] attlist = [.. AdditionalAttributeNamesPerElement[name]];
                    return
                        $"<!ELEMENT {name} EMPTY>{Environment.NewLine}<!ATTLIST {name}{Environment.NewLine}\t{string.Join(Environment.NewLine + "\t", attlist.Select(attribute => $"{attribute} CDATA #IMPLIED").ToArray())}>{Environment.NewLine}";
                }).ToArray())
                : string.Empty;
            dtd += """
                   <!ELEMENT XPathToUow (#PCDATA)>
                   <!ELEMENT Filename (#PCDATA)>
                   <!ELEMENT State (Value, StateName, Remark)>
                   <!ELEMENT Value (#PCDATA)>
                   <!ELEMENT StateName (#PCDATA)>
                   <!ELEMENT Remark (#PCDATA)>
                   <!ENTITY % HTMLlat1 PUBLIC "-//W3C//ENTITIES Latin 1 for XHTML//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml-lat1.ent">
                   """;
            _ = xmlReport.AppendChild(xmlReport.CreateDocumentType("XmlSplitReport", null, null, dtd));
            _ = xmlReport.AppendChild(root);
            foreach (var entry in this)
            {
                var xmlEntry = xmlReport.CreateElement("Entry");
                _ = root.AppendChild(xmlEntry);
                var checkoutNumber = xmlReport.CreateAttribute("CheckoutNumber");
                checkoutNumber.Value = entry.CheckoutParentNumber.ToString();
                _ = xmlEntry.Attributes.Append(checkoutNumber);
                var nodeNumber = xmlReport.CreateAttribute("NodeNumber");
                nodeNumber.Value = entry.NodeNumber.ToString();
                _ = xmlEntry.Attributes.Append(nodeNumber);
                var checkoutKey = xmlReport.CreateAttribute("CheckoutKey");
                checkoutKey.Value = entry.CheckoutParent.Attributes?["key"]?.Value;
                _ = xmlEntry.Attributes.Append(checkoutKey);
                var uowKey = xmlReport.CreateAttribute("UowKey");
                uowKey.Value = entry.UowNode.Attributes?["key"]?.Value;
                _ = xmlEntry.Attributes.Append(uowKey);
                var checkoutName = xmlReport.CreateElement("CheckoutName");
                checkoutName.InnerText = entry.CheckoutParent.Name;
                _ = xmlEntry.AppendChild(checkoutName);
                var nodeName = xmlReport.CreateElement("NodeName");
                nodeName.InnerText = entry.UowNode.Name;
                _ = xmlEntry.AppendChild(nodeName);
                var parents = xmlReport.CreateElement("Ancestors");
                // append a child element to parents with only the name and attributes of entry.KeyedParent, but none of its content

                var keyedParent = xmlReport.CreateElement(entry.KeyedParent.Name);
                foreach (XmlAttribute attribute in entry.KeyedParent.Attributes!)
                    keyedParent.SetAttribute(attribute.Name, attribute.Value);
                _ = parents.AppendChild(keyedParent);
                if (entry.CheckoutParent.OuterXml != entry.KeyedParent.OuterXml)
                {
                    var checkoutParent = xmlReport.CreateElement(entry.CheckoutParent.Name);
                    var checkoutComment = xmlReport.CreateComment(
                        "The checkout node below is the parent container (to the unit-of-work that is in progress) which is permitted to be exported.");
                    foreach (XmlAttribute attribute in entry.CheckoutParent.Attributes!)
                        checkoutParent.SetAttribute(attribute.Name, attribute.Value);
                    _ = parents.AppendChild(checkoutComment);
                    _ = parents.AppendChild(checkoutParent);
                }

                _ = xmlEntry.AppendChild(parents);
                var xpath = xmlReport.CreateElement("XPathToUow");
                xpath.InnerText = entry.FullXPath;
                var xpathComment =
                    xmlReport.CreateComment("The XPath below is for the unit-of-work node, not the checkout node.");
                _ = xmlEntry.AppendChild(xpathComment);
                _ = xmlEntry.AppendChild(xpath);
                var filename = xmlReport.CreateElement("Filename");
                filename.InnerText = entry.FilenameOfSplit;
                _ = xmlEntry.AppendChild(filename);

                #region UowState

                var state = xmlReport.CreateElement("State");
                _ = xmlEntry.AppendChild(state);
                var stateValue = xmlReport.CreateElement("Value");
                stateValue.InnerText = entry.UowState.StateValue.ToString() ?? "&nbsp;";
                var stateName = xmlReport.CreateElement("StateName");
                stateName.InnerText = entry.UowState.StateName ?? "&nbsp;";
                var stateRemark = xmlReport.CreateElement("Remark");
                stateRemark.InnerText = entry.UowState.Remark ?? "&nbsp;";
                foreach (var statePartial in new[] { stateValue, stateName, stateRemark })
                    _ = state.AppendChild(statePartial);

                #endregion UowState
            }

            xmlReport.Save(outPath);
        }
    }
}

/// <summary>
///     An entry per unit of work in the final report on splitting the source XML.
/// </summary>
public class XmlSplitReportEntry
{
    /// <summary>
    ///     The checkout parent
    /// </summary>
    private XmlNode checkoutParent;

    /// <summary>
    ///     The checkout parent number
    /// </summary>
    public int CheckoutParentNumber;

    /// <summary>
    ///     The filename of split
    /// </summary>
    public string FilenameOfSplit;

    /// <summary>
    ///     The full XPath
    /// </summary>
    public string FullXPath;

    /// <summary>
    ///     The keyed parent
    /// </summary>
    private XmlNode keyedParent;

    /// <summary>
    ///     The node number
    /// </summary>
    public int NodeNumber;

    /// <summary>
    ///     The uow node
    /// </summary>
    public XmlNode UowNode;

    /// <summary>
    ///     The uow state
    /// </summary>
    public UowState UowState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="XmlSplitReportEntry" /> class.
    /// </summary>
    /// <param name="checkoutParent">The checkout parent.</param>
    /// <param name="checkoutParentNumber">The checkout parent number.</param>
    /// <param name="nodeNumber">The node number.</param>
    /// <param name="uowNode">The uow node.</param>
    /// <param name="keyedParentTag">The keyed parent tag.</param>
    /// <param name="fullXPath">The full x path.</param>
    /// <param name="filenameOfSplit">The filename of split.</param>
    /// <param name="uowState">State of the uow.</param>
    public XmlSplitReportEntry(XmlNode checkoutParent, int checkoutParentNumber, int nodeNumber, XmlNode uowNode,
        XmlNode keyedParentTag, string fullXPath, string filenameOfSplit, UowState uowState)
    {
        CheckoutParentNumber = checkoutParentNumber;
        NodeNumber = nodeNumber;
        UowNode = uowNode;
        this.checkoutParent = CheckoutParent = checkoutParent;
        keyedParent = KeyedParent = keyedParentTag;
        FullXPath = fullXPath;
        FilenameOfSplit = filenameOfSplit;
        UowState = uowState;
    }

    /// <summary>
    ///     Gets or sets the keyed parent.
    /// </summary>
    /// <value>
    ///     The keyed parent.
    /// </value>
    public XmlNode KeyedParent
    {
        get => NodeToTag(keyedParent);
        set => keyedParent = value;
    }

    /// <summary>
    ///     Gets or sets the checkout parent.
    /// </summary>
    /// <value>
    ///     The checkout parent.
    /// </value>
    public XmlNode CheckoutParent
    {
        get => NodeToTag(checkoutParent);
        set => checkoutParent = value;
    }

    /// <summary>
    ///     Nodes to tag.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns></returns>
    private static XmlElement NodeToTag(XmlNode node)
    {
        var element = new XmlDocument().CreateElement(node.Name);
        foreach (XmlAttribute attribute in node.Attributes!) element.SetAttribute(attribute.Name, attribute.Value);
        return element;
    }
}

/// <summary>
///     A generic interface for XML split reporting where the category name is derived from the specified
///     <typeparamref name="TCategoryName" /> type name.
///     Generally used to enable activation of a named <see cref="IXmlSplitReport" /> from dependency injection.
/// </summary>
/// <typeparam name="TCategoryName">The type whose name is used for the XML split report category name.</typeparam>
public interface IXmlSplitReport<out TCategoryName> : IXmlSplitReport
{
}

/// <summary>
///     Generic type for the XML splitting result report in XML, CSV, and TSV.
/// </summary>
public interface IXmlSplitReport : IProducerConsumerCollection<XmlSplitReportEntry>
{
    /// <summary>
    ///     Gets or sets the base name <see cref="XmlSplitter.XmlSourceFileBaseName" /> of the report.
    /// </summary>
    /// <value>
    ///     The base name of the report.
    /// </value>
    string BaseName { get; set; }

    /// <summary>
    ///     Gets the report name.
    /// </summary>
    /// <value>
    ///     The name.
    /// </value>
    string Name { get; }

    /// <summary>
    ///     Gets the node number.
    /// </summary>
    /// <value>
    ///     The node number.
    /// </value>
    int NodeNumber { get; }

    /// <summary>
    ///     Asynchronous callback when an entry is added to the report.
    /// </summary>
    public event Action? OnEntryAdded;

    /// <summary>
    ///     Adds the <see cref="XmlSplitReportEntry" /> entry.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="checkoutParent">The checkout parent.</param>
    /// <param name="checkoutParentNumber">The checkout parent number.</param>
    /// <param name="uowState">State of the uow.</param>
    /// <returns></returns>
    void AddEntry(XmlNode node, XmlNode checkoutParent, int checkoutParentNumber, UowState uowState);

    /// <summary>
    ///     Adds the <see cref="XmlSplitReportEntry" /> entries.
    /// </summary>
    /// <param name="children">The children.</param>
    /// <param name="checkoutParent">The checkout parent.</param>
    /// <param name="checkoutParentNumber">The checkout parent number.</param>
    /// <returns></returns>
    void AddEntries(IEnumerable<StateWithNode> children, XmlNode checkoutParent, int checkoutParentNumber);

    /// <summary>
    ///     Gets the number of siblings for a given <see cref="XmlSplitReportEntry" />.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <returns>The number of siblings.</returns>
    int GetNumSiblings(XmlSplitReportEntry entry);

    /// <summary>
    ///     Generates the unique XPath for the XML node.
    /// </summary>
    /// <param name="xmlNode">The XML node.</param>
    /// <returns>An XPath for the given node.</returns>
    public static string GenerateUniqueXPath(XmlNode xmlNode)
    {
        if (xmlNode.NodeType == XmlNodeType.Attribute && xmlNode is XmlAttribute
            {
                OwnerElement: not null
            } xmlNodeAsAttribute)
            // attributes have an OwnerElement, not a ParentNode; also they have             
            // to be matched by name, not found by position
            return $"{GenerateUniqueXPath(xmlNodeAsAttribute.OwnerElement)}/@{xmlNode.Name}";
        if (xmlNode.ParentNode is null)
            // the only node with no parent is the root node, which has no path
            return string.Empty;

        // Get the Index
        var indexInParent = 1;
        var siblingNode = xmlNode.PreviousSibling;
        // Loop through all Siblings
        while (siblingNode is not null)
        {
            // Increase the Index if the Sibling has the same Name
            if (siblingNode.Name == xmlNode.Name) indexInParent++;
            siblingNode = siblingNode.PreviousSibling;
        }

        // the path to a node is the path to its parent, plus "/node()[n]", where n is its position among its siblings.         
        return $"{GenerateUniqueXPath(xmlNode.ParentNode)}/{xmlNode.Name}[{indexInParent}]";
    }

    /// <summary>
    ///     Saves the report asynchronously.
    /// </summary>
    /// <param name="outPath">The out path.</param>
    /// <param name="outFormat">The output format.</param>
    /// <returns></returns>
    Task SaveAsync(string outPath, XmlSplitReport.ReportFormat outFormat);

    protected const string ReportTimestampFormat = "yyyy - MM - dd - HH - mm - ss - fffffff";
}