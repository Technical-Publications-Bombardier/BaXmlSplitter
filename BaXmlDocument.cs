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
                using var reader = XmlReader.Create(new StringReader(xml), settings);
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
                using var reader = XmlReader.Create(inStream, settings);
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
                using var reader = XmlReader.Create(filename, settings);
                base.Load(reader);
            }
        }

        public XmlNode[]? SelectNodesByCheckout(string xpath, string[] checkoutNames)
        {
            List<XmlNode> selectedNodes = [];
            if (SelectNodes(xpath) is not { } nodes) return [.. selectedNodes];
            HashSet<XmlNode> notAlreadySeen = new(nodes.Count);
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] is null || nodes[i] is not { } node || !notAlreadySeen.Add(node)) continue;
                if (checkoutNames.Contains(node.Name, StringComparer.OrdinalIgnoreCase))
                {
                    selectedNodes.Add(node);
                    continue;
                }
                if (node.ParentNode is not { } ancestor) continue;
                while (ancestor.Name is { } ancestorName && !checkoutNames.Contains(ancestorName, StringComparer.OrdinalIgnoreCase) && ancestor.ParentNode is { } greatAncestor)
                {
                    ancestor = greatAncestor;
                }
                if (notAlreadySeen.Add(ancestor) && checkoutNames.Contains(ancestor.Name, StringComparer.OrdinalIgnoreCase))
                {
                    selectedNodes.Add(ancestor);
                }
            }
            return [.. selectedNodes];
        }
    }
}