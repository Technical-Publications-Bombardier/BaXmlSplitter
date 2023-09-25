using System.Linq;
using System.Xml;

namespace BaXmlSplitter
{
    internal class XmlSplitReport(int capacity = XmlSplitReport.DEFAULT_CAPACITY)
    {
        private XmlSplitReportEntry[] _entries = new XmlSplitReportEntry[capacity];
        private XmlSplitReportEntry[] Entries { get => _entries.Take(lastIndex).ToArray(); set => _entries = value; }
        private string[] ParentTagNames => Entries.SelectMany(entry => new[] { entry.CheckoutParent.Name, entry.KeyedParent.Name }).Distinct().ToArray();
        private Dictionary<string, HashSet<string>> AdditionalAttributeNamesPerElement
        {
            get
            {
                Dictionary<string, HashSet<string>> builder = [];
                foreach (string elementName in ParentTagNames)
                {
                    builder.Add(elementName, [
                        .. Entries.Where(entry => entry.KeyedParent.Name == elementName).SelectMany(entry => entry.KeyedParent.Attributes!.Cast<XmlAttribute>().Select(attrib => attrib.Name)),
                        .. Entries.Where(entry => entry.CheckoutParent.Name == elementName).SelectMany(entry => entry.CheckoutParent.Attributes!.Cast<XmlAttribute>().Select(attrib => attrib.Name))
                        ]);
                }
                return builder;
            }
        }
        private int lastIndex = 0;
        private const int DEFAULT_CAPACITY = 10;
        private static readonly string[] FIELDS = ["CheckoutParentNumber", "CheckoutParentName", "CheckoutParentKey", "CheckoutParentTag", "UowNumber", "UowElementName", "UowKeyValue", "KeyedParent", "FullXPathToUow", "FilenameOfSplit"];

        internal enum ReportFormat
        {
            CSV,
            TSV,
            XML
        }
        public XmlSplitReport() : this(DEFAULT_CAPACITY)
        {

        }

        public void Add(XmlSplitReportEntry entry)
        {
            if (lastIndex + 1 >= _entries.Length)
            {
                Array.Resize(ref _entries, 2 * _entries.Length);
            }
            _entries[lastIndex++] = entry;
        }
        public void Save(string outPath, ReportFormat outFormat)
        {
            switch (outFormat)
            {
                case ReportFormat.CSV:
                case ReportFormat.TSV:
                    {
                        using StreamWriter writer = new(outPath);
                        char separator = outFormat == ReportFormat.CSV ? ',' : '\t';
                        writer.WriteLine(string.Join(separator, FIELDS));
                        foreach (XmlSplitReportEntry entry in Entries)
                        {
                            string line = string.Join(separator, new string[] { entry.CheckoutParentNumber.ToString(), entry.CheckoutParent.Name, entry.CheckoutParent.Attributes?["key"]?.ToString() ?? "&nbsp;", entry.CheckoutParent.OuterXml, entry.NodeNumber.ToString(), entry.UowNode.Name.ToString(), entry.UowNode.Attributes?["key"]?.ToString() ?? "&nbsp;", entry.KeyedParent.OuterXml, entry.FullXPath, entry.FilenameOfSplit });
                            writer.WriteLine(line);
                        }
                        break;
                    }
                case ReportFormat.XML:
                    {
                        XmlDocument xmlReport = new();
                        XmlElement root = xmlReport.CreateElement("XmlSplitReport");
                        string dtd = """
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
                        dtd += $"<!ELEMENT Ancestors ({(ParentTagNames.Length > 0 ? string.Join('|', ParentTagNames) : "EMPTY")})*>";
                        dtd += Environment.NewLine;
                        dtd += AdditionalAttributeNamesPerElement.Count > 0 ? string.Join(Environment.NewLine, ParentTagNames.Select(name => { string[] attlist = [.. AdditionalAttributeNamesPerElement[name]]; return $"<!ELEMENT {name} EMPTY>{Environment.NewLine}<!ATTLIST {name}{Environment.NewLine}\t{string.Join(Environment.NewLine + "\t", attlist.Select(attribute => $"{attribute} CDATA #IMPLIED").ToArray())}>{Environment.NewLine}"; }).ToArray()) : string.Empty;
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
                        foreach (XmlSplitReportEntry entry in Entries)
                        {
                            XmlElement xmlEntry = xmlReport.CreateElement("Entry");
                            _ = root.AppendChild(xmlEntry);
                            XmlAttribute checkoutNumber = xmlReport.CreateAttribute("CheckoutNumber");
                            checkoutNumber.Value = entry.CheckoutParentNumber.ToString();
                            _ = xmlEntry.Attributes.Append(checkoutNumber);
                            XmlAttribute nodeNumber = xmlReport.CreateAttribute("NodeNumber");
                            nodeNumber.Value = entry.NodeNumber.ToString();
                            _ = xmlEntry.Attributes.Append(nodeNumber);
                            XmlAttribute checkoutKey = xmlReport.CreateAttribute("CheckoutKey");
                            checkoutKey.Value = entry.CheckoutParent.Attributes?["key"]?.Value;
                            _ = xmlEntry.Attributes.Append(checkoutKey);
                            XmlAttribute uowKey = xmlReport.CreateAttribute("UowKey");
                            uowKey.Value = entry.UowNode.Attributes?["key"]?.Value;
                            _ = xmlEntry.Attributes.Append(uowKey);
                            XmlElement checkoutName = xmlReport.CreateElement("CheckoutName");
                            checkoutName.InnerText = entry.CheckoutParent.Name;
                            _ = xmlEntry.AppendChild(checkoutName);
                            XmlElement nodeName = xmlReport.CreateElement("NodeName");
                            nodeName.InnerText = entry.UowNode.Name;
                            _ = xmlEntry.AppendChild(nodeName);
                            XmlElement parents = xmlReport.CreateElement("Ancestors");
                            // append a child element to parents with only the name and attributes of entry.KeyedParent, but none of its content

                            XmlElement keyedParent = xmlReport.CreateElement(entry.KeyedParent.Name);
                            foreach (XmlAttribute attrib in entry.KeyedParent.Attributes!)
                            {
                                keyedParent.SetAttribute(attrib.Name, attrib.Value);
                            }
                            _ = parents.AppendChild(keyedParent);
                            if (entry.CheckoutParent.OuterXml != entry.KeyedParent.OuterXml)
                            {
                                XmlElement checkoutParent = xmlReport.CreateElement(entry.CheckoutParent.Name);
                                XmlComment checkoutComment = xmlReport.CreateComment("The checkout node below is the parent container (to the unit-of-work that is in progress) which is permitted to be exported.");
                                foreach (XmlAttribute attrib in entry.CheckoutParent.Attributes!)
                                {
                                    checkoutParent.SetAttribute(attrib.Name, attrib.Value);
                                }
                                _ = parents.AppendChild(checkoutComment);
                                _ = parents.AppendChild(checkoutParent);
                            }
                            _ = xmlEntry.AppendChild(parents);
                            XmlElement xpath = xmlReport.CreateElement("XPathToUow");
                            xpath.InnerText = entry.FullXPath;
                            XmlComment xpathComment = xmlReport.CreateComment("The XPath below is for the unit-of-work node, not the checkout node.");
                            _ = xmlEntry.AppendChild(xpathComment);
                            _ = xmlEntry.AppendChild(xpath);
                            XmlElement filename = xmlReport.CreateElement("Filename");
                            filename.InnerText = entry.FilenameOfSplit;
                            _ = xmlEntry.AppendChild(filename);

                            #region UowState
                            XmlElement state = xmlReport.CreateElement("State");
                            _ = xmlEntry.AppendChild(state);
                            XmlElement stateValue = xmlReport.CreateElement("Value");
                            stateValue.InnerText = entry.UowState.StateValue.ToString() is string stateValueStr ? stateValueStr : "&nbsp;";
                            XmlElement stateName = xmlReport.CreateElement("StateName");
                            stateName.InnerText = entry.UowState.StateName is string stateNameStr ? stateNameStr : "&nbsp;";
                            XmlElement stateRemark = xmlReport.CreateElement("Remark");
                            stateRemark.InnerText = entry.UowState.Remark is string remarkStr ? remarkStr : "&nbsp;";
                            foreach (XmlElement statePartial in new XmlElement[] { stateValue, stateName, stateRemark })
                            {
                                _ = state.AppendChild(statePartial);
                            }
                            #endregion UowState
                        }
                        xmlReport.Save(outPath);
                        break;
                    }
                default: break;
            }
        }
    }
    internal class XmlSplitReportEntry
    {
        public int NodeNumber;
        public int CheckoutParentNumber;
        private static XmlElement NodeToTag(XmlNode node)
        {

            XmlElement element = new XmlDocument().CreateElement(node.Name);
            foreach (XmlAttribute attrib in node.Attributes!)
            {
                element.SetAttribute(attrib.Name, attrib.Value);
            }
            return element;
        }

        public XmlNode KeyedParent { get => NodeToTag(_keyedParent); set => _keyedParent = value; }
        public XmlNode CheckoutParent { get => NodeToTag(_checkoutParent); set => _checkoutParent = value; }
        public XmlNode UowNode;
        public string FullXPath;
        public string FilenameOfSplit;
        public UowState UowState;
        private XmlNode _keyedParent;
        private XmlNode _checkoutParent;

        public XmlSplitReportEntry(XmlNode checkoutParent, int checkoutParentNumber, int nodeNumber, XmlNode uowNode, XmlNode keyedParentTag, string fullXPath, string filenameOfSplit, UowState uowState)
        {
            CheckoutParentNumber = checkoutParentNumber;
            NodeNumber = nodeNumber;
            UowNode = uowNode;
            _checkoutParent = CheckoutParent = checkoutParent;
            _keyedParent = KeyedParent = keyedParentTag;
            FullXPath = fullXPath;
            FilenameOfSplit = filenameOfSplit;
            UowState = uowState;
        }

    }

}