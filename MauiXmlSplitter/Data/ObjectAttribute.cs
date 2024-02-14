using System.ComponentModel.DataAnnotations;

namespace BaXmlSplitter
{
    public class ObjectAttribute
    {
        [Required]
        public int ObjectRef { get; set; }

        [Required]
        public string AttributeName { get; set; }

        public string AttributeValue { get; set; }
    }
}