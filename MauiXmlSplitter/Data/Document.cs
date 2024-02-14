using System.ComponentModel.DataAnnotations;

namespace BaXmlSplitter
{
    public class Document
    {
        [Required]
        public int DocumentRef { get; set; }

        [Required]
        public string ObjectPath { get; set; }

        [Required]
        public int ObjectRef { get; set; }
    }
}
