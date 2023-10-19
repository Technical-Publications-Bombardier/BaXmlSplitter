using System.Web;
using System.Xml;

namespace BaXmlSplitter
{
    internal class XmlSplitReport
    {
        private XmlSplitReportEntry[] _entries = new XmlSplitReportEntry[DEFAULT_CAPACITY];
        private XmlSplitReportEntry[] Entries { get => _entries.Take(lastIndex).ToArray(); set => _entries = value; }
        private int lastIndex = 0;
        private const int DEFAULT_CAPACITY = 10;
        private static readonly string[] FIELDS = new string[] { "NodeNumber", "NodeElementName", "KeyValue", "ImmediateParentOpeningTag", "FullXPath", "FilenameOfSplit" };

        internal enum ReportFormat
        {
            CSV,
            TSV,
            XML
        }
        public XmlSplitReport() : this(DEFAULT_CAPACITY)
        {

        }
        public XmlSplitReport(int capacity = DEFAULT_CAPACITY)
        {
            _entries = new XmlSplitReportEntry[capacity];
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
                            string line = string.Join(separator, new string[] { entry.NodeNumber.ToString(), entry.NodeElementName, entry.KeyValue, entry.ImmediateParentOpeningTag, entry.FullXPath, entry.FilenameOfSplit });
                            writer.WriteLine(line);
                        }
                        break;
                    }
                case ReportFormat.XML:
                    {
                        XmlDocument xmlReport = new();
                        XmlElement root = xmlReport.CreateElement("XmlSplitReport");
                        const string dtd = """
                        <!ELEMENT XmlSplitReport (Entry*)>
                        <!ELEMENT Entry (Name, ParentTag, XPath, Filename, State)>
                        <!ATTLIST Entry
                                  NodeNumber CDATA #REQUIRED
                                  Key CDATA #REQUIRED>
                        <!ELEMENT Name (#PCDATA)>
                        <!ELEMENT ParentTag (#PCDATA)>
                        <!ELEMENT XPath (#PCDATA)>
                        <!ELEMENT Filename (#PCDATA)>
                        <!ELEMENT State (#PCDATA)>
                        <!ATTLIST State
                                  Value CDATA #REQUIRED
                                  Name CDATA #REQUIRED
                                  Remark CDATA #IMPLIED>
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
                            XmlElement name = xmlReport.CreateElement("Name");
                            name.InnerText = entry.NodeElementName;
                            _ = xmlEntry.AppendChild(name);
                            XmlElement parentTag = xmlReport.CreateElement("ParentTag");
                            parentTag.InnerText = entry.ImmediateParentOpeningTag;
                            _ = xmlEntry.AppendChild(parentTag);
                            XmlElement xpath = xmlReport.CreateElement("XPath");
                            xpath.InnerText = entry.FullXPath;
                            _ = xmlEntry.AppendChild(xpath);
                            XmlElement filename = xmlReport.CreateElement("Filename");
                            filename.InnerText = entry.FilenameOfSplit;
                            _ = xmlEntry.AppendChild(filename);
                            XmlElement state = xmlReport.CreateElement("State");
                            _ = xmlEntry.AppendChild(state);
                            XmlAttribute value = xmlReport.CreateAttribute("Value");
                            value.Value = entry.UowState.StateValue.ToString();
                            _ = state.Attributes.Append(value);
                            XmlAttribute nameAttribute = xmlReport.CreateAttribute("Name");
                            nameAttribute.Value = entry.UowState.StateName;
                            _ = state.Attributes.Append(nameAttribute);
                            if (!string.IsNullOrEmpty(entry.UowState.Remark))
                            {
                                XmlAttribute remark = xmlReport.CreateAttribute("Remark");
                                remark.Value = HttpUtility.HtmlEncode(entry.UowState.Remark);
                                _ = state.Attributes.Append(remark);
                            }
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
        public string ImmediateParentOpeningTag;
        public string FullXPath;
        public string FilenameOfSplit;
        public UowState UowState;

        public XmlSplitReportEntry(int nodeNumber, string nodeElementName, string keyValue, string immediateParentOpeningTag, string fullXPath, string filenameOfSplit, UowState uowState)
        {
            NodeNumber = nodeNumber;
            NodeElementName = nodeElementName;
            KeyValue = keyValue;
            ImmediateParentOpeningTag = immediateParentOpeningTag;
            FullXPath = fullXPath;
            FilenameOfSplit = filenameOfSplit;
            UowState = uowState;
        }

    }

}