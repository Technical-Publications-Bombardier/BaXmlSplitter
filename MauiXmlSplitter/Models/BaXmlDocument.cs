using System.Xml;

namespace MauiXmlSplitter.Models
{
    /// <summary>
    /// An XML document with entity resolution disabled.
    /// </summary>
    /// <seealso cref="System.Xml.XmlDocument" />
    public class BaXmlDocument : XmlDocument
    {
        /// <summary>
        /// Determines whether to resolve character entities on XML load.
        /// </summary>
        public bool ResolveEntities { get; init; } = true;

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <summary>
        /// Selects the nodes by filtering the node name against an array of strings.
        /// If the node name is in the array, the node is included. Otherwise, the node
        /// is traversed up to the most recent ancestor whose name is in the array.
        /// </summary>
        /// <param name="xpath">The xpath.</param>
        /// <param name="checkoutNames">The checkout names.</param>
        /// <returns></returns>
        public XmlNode[]? SelectNodesByCheckout(string xpath, string[] checkoutNames)
        {
            HashSet<XmlNode> selectedNodes = [];
            if (SelectNodes(xpath) is not { } nodes) return [.. selectedNodes];
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] is null || nodes[i] is not { } node || selectedNodes.Contains(node)) continue;
                if (checkoutNames.Contains(node.Name, StringComparer.OrdinalIgnoreCase))
                {
                    selectedNodes.Add(node);
                }
            }
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] is null || nodes[i] is not { } node || selectedNodes.Contains(node)) continue;
                if (node.ParentNode is not { } ancestor) continue;
                while (ancestor.Name is { } ancestorName && !checkoutNames.Contains(ancestorName, StringComparer.OrdinalIgnoreCase) && ancestor.ParentNode is { } greatAncestor)
                {
                    ancestor = greatAncestor;
                }
                if (checkoutNames.Contains(ancestor.Name, StringComparer.OrdinalIgnoreCase))
                {
                    _ = selectedNodes.Add(ancestor);
                }
            }
            return [.. selectedNodes];
        }
    }
}