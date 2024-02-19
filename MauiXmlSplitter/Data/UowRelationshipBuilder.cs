using System.Linq;
using System.Xml;
using Microsoft.EntityFrameworkCore;

namespace MauiXmlSplitter.Data
{
    /// <summary>
    /// Build the child-parent relationship between units-of-work.
    /// </summary>
    public class UowRelationshipBuilder(ManualContext context)
    {
        /// <summary>
        /// Constructs the XML.
        /// </summary>
        /// <param name="rootObjectRef">The root object reference.</param>
        /// <returns></returns>
        public XmlDocument ConstructXml(int rootObjectRef)
        {
            var xmlDoc = new XmlDocument();
            var rootNode = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(rootNode);

            AppendChildren(rootNode, rootObjectRef);

            return xmlDoc;
        }

        /// <summary>
        /// Appends the children.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="parentObjectRef">The parent object reference.</param>
        private void AppendChildren(XmlNode parentNode, int parentObjectRef)
        {
            var children = context.ObjectNew
                .Include(o => o.ObjectAttributes)
                .Where(o => o.ParentObjectId == parentObjectRef)
                .ToList();

            foreach (var child in children)
            {
                if (parentNode.OwnerDocument?.CreateElement("entity") is not { } childNode)
                    continue;
                parentNode.AppendChild(childNode);

                foreach (var property in typeof(ObjectNew).GetProperties())
                {
                    var attribute = childNode.OwnerDocument.CreateAttribute(property.Name);
                    attribute.Value = property.GetValue(child)?.ToString() ?? string.Empty;
                    childNode.Attributes.Append(attribute);
                }

                if (child.ObjectAttributes == null)
                    continue;

                foreach (var objectAttribute in child.ObjectAttributes)
                {
                    var attributeNode = childNode.OwnerDocument.CreateElement("attribute");
                    childNode.AppendChild(attributeNode);

                    foreach (var property in typeof(ObjectAttribute).GetProperties())
                    {
                        var attribute = attributeNode.OwnerDocument.CreateAttribute(property.Name);
                        attribute.Value = property.GetValue(objectAttribute)?.ToString() ?? string.Empty;
                        attributeNode.Attributes.Append(attribute);
                    }
                }

                AppendChildren(childNode, child.ObjectRef);
            }
        }
    }
}
