using System.Xml;

namespace BaXmlSplitter
{
    internal class XmlSplitReport(int capacity = XmlSplitReport.DEFAULT_CAPACITY)
    {
        private XmlSplitReportEntry[] _entries = new XmlSplitReportEntry[capacity];
        private XmlSplitReportEntry[] Entries { get => _entries.Take(lastIndex).ToArray(); set => _entries = value; }
        private string[] AdditionalElementNames => Entries.Select(entry => entry.KeyedParentTag.Name).Distinct().ToArray();
        private Dictionary<string, string[]> AdditionalAttributeNamesPerElement
        {
            get
            {
                Dictionary<string, string[]> builder = [];
                foreach (string elementName in AdditionalElementNames)
                {
                    builder.Add(elementName, Entries.Where(entry => entry.KeyedParentTag.Name == elementName).SelectMany(entry => entry.KeyedParentTag.Attributes!.Cast<XmlAttribute>().Select(attrib => attrib.Name)).Distinct().ToArray());
                }
                return builder;
            }
        }
        private int lastIndex = 0;
        private const int DEFAULT_CAPACITY = 10;
        private static readonly string[] FIELDS = ["NodeNumber", "NodeElementName", "KeyValue", "KeyedParentTag", "FullXPath", "FilenameOfSplit"];

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
                            string line = string.Join(separator, new string[] { entry.NodeNumber.ToString(), entry.NodeElementName, entry.KeyValue, entry.KeyedParentTag.OuterXml, entry.FullXPath, entry.FilenameOfSplit });
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
                        <!ELEMENT Entry (NodeName, ParentTag, XPath, Filename, State)>
                        <!ATTLIST Entry
                                  NodeNumber CDATA #REQUIRED
                                  Key CDATA #REQUIRED>
                        <!ELEMENT NodeName (#PCDATA)>
                        """;
                        dtd += Environment.NewLine;
                        dtd += $"<!ELEMENT ParentTag ({(AdditionalElementNames.Length > 0 ? string.Join('|', AdditionalElementNames) : "EMPTY")})>";
                        dtd += Environment.NewLine;
                        dtd += AdditionalAttributeNamesPerElement.Count > 0 ? string.Join(Environment.NewLine, AdditionalElementNames.Select(name => { string[] attlist = AdditionalAttributeNamesPerElement[name]; return $"<!ELEMENT {name} EMPTY>{Environment.NewLine}<!ATTLIST {name}{Environment.NewLine}\t{string.Join(Environment.NewLine + "\t", attlist.Select(attribute => $"{attribute} CDATA #IMPLIED").ToArray())}>{Environment.NewLine}"; }).ToArray()) : string.Empty;
                        dtd += """
                        <!ELEMENT XPath (#PCDATA)>
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
                            XmlAttribute nodeNumber = xmlReport.CreateAttribute("NodeNumber");
                            nodeNumber.Value = entry.NodeNumber.ToString();
                            _ = xmlEntry.Attributes.Append(nodeNumber);
                            XmlAttribute key = xmlReport.CreateAttribute("Key");
                            key.Value = entry.KeyValue;
                            _ = xmlEntry.Attributes.Append(key);
                            XmlElement nodeName = xmlReport.CreateElement("NodeName");
                            nodeName.InnerText = entry.NodeElementName;
                            _ = xmlEntry.AppendChild(nodeName);
                            XmlElement parentTag = xmlReport.CreateElement("ParentTag");
                            // append a child element to parentTag with only the name and attributes of entry.KeyedParentTag, but none of its content
                            XmlElement keyedParentTag = xmlReport.CreateElement(entry.KeyedParentTag.Name);
                            foreach (XmlAttribute attrib in entry.KeyedParentTag.Attributes!)
                            {
                                keyedParentTag.SetAttribute(attrib.Name, attrib.Value);
                            }
                            _ = parentTag.AppendChild(keyedParentTag);
                            _ = xmlEntry.AppendChild(parentTag);
                            XmlElement xpath = xmlReport.CreateElement("XPath");
                            xpath.InnerText = entry.FullXPath;
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
        public string NodeElementName;
        public string KeyValue;
        public XmlNode KeyedParentTag
        {
            get
            {
                XmlElement element = new XmlDocument().CreateElement(_keyedParentTag.Name);
                foreach (XmlAttribute attrib in _keyedParentTag.Attributes!)
                {
                    element.SetAttribute(attrib.Name, attrib.Value);
                }
                return element;
            }
            set => _keyedParentTag = value;
        }
        public string FullXPath;
        public string FilenameOfSplit;
        public UowState UowState;
        private XmlNode _keyedParentTag;

        public XmlSplitReportEntry(int nodeNumber, string nodeElementName, string keyValue, XmlNode keyedParentTag, string fullXPath, string filenameOfSplit, UowState uowState)
        {
            NodeNumber = nodeNumber;
            NodeElementName = nodeElementName;
            KeyValue = keyValue;
            _keyedParentTag = KeyedParentTag = keyedParentTag;
            FullXPath = fullXPath;
            FilenameOfSplit = filenameOfSplit;
            UowState = uowState;
        }

    }

}