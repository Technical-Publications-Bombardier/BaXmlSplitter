using System.ComponentModel.DataAnnotations;

namespace MauiXmlSplitter.Data
{
    public class ObjectAttribute
    {
        [Required]
        public int ObjectRef { get; set; }

        [Required]
        public string AttributeName { get; set; }

        public string AttributeValue { get; set; }
        // Navigation property
        public ObjectNew? ObjectNew { get; set; }
    }
}