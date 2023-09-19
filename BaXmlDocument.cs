using System.Collections;
using System.Collections.ObjectModel;
using System.Xml;

namespace BaXmlSplitter
{
    internal class BaXmlDocument : XmlDocument
    {
        public bool ResolveEntities { get; set; } = true;


        public override void LoadXml(string xml)
        {
            if (ResolveEntities)
            {
                base.LoadXml(xml);
            }
            else
            {
                XmlReaderSettings settings = new()
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null
                };
                using XmlReader reader = XmlReader.Create(new StringReader(xml), settings);
                base.Load(reader);
            }
        }
        public override void Load(Stream inStream)
        {
            if (ResolveEntities)
            {
                base.Load(inStream);
            }
            else
            {
                XmlReaderSettings settings = new()
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null
                };
                using XmlReader reader = XmlReader.Create(inStream, settings);
                base.Load(reader);
            }
        }

        public override void Load(string filename)
        {
            if (ResolveEntities)
            {
                base.Load(filename);
            }
            else
            {
                XmlReaderSettings settings = new()
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null
                };
                using XmlReader reader = XmlReader.Create(filename, settings);
                base.Load(reader);
            }
        }

        public XmlNode[]? SelectNodesByCheckout(string xpath, string[] checkoutNames)
        {
            List<XmlNode> selectedNodes = [];
            if (SelectNodes(xpath) is XmlNodeList nodes)
            {
                HashSet<XmlNode> notAlreadySeen = new(nodes.Count);
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] != null && nodes[i] is XmlNode node && notAlreadySeen.Add(node) && !checkoutNames.Contains(node.Name, StringComparer.OrdinalIgnoreCase) && node.ParentNode is XmlNode ancestor)
                    {
                        while (ancestor != null && ancestor.Name is string ancestorName && !checkoutNames.Contains(ancestorName, StringComparer.OrdinalIgnoreCase) && ancestor.ParentNode is XmlNode greatAncestor)
                        {
                            ancestor = greatAncestor;
                        }
                        if (ancestor != null && notAlreadySeen.Add(ancestor) && checkoutNames.Contains(ancestor.Name, StringComparer.OrdinalIgnoreCase))
                        {
                            selectedNodes.Add(ancestor);
                        }
                        else
                        {
                            selectedNodes.Add(node);
                        }
                    }
                }
            }
            return [.. selectedNodes];
        }
    }
}