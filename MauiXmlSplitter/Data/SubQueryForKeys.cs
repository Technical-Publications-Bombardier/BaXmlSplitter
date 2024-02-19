using System.ComponentModel.DataAnnotations;

namespace MauiXmlSplitter.Data
{
    public class SubQueryForKeys
    {
        [Required]
        public int ObjectId { get; set; }
        [Required]
        public string Key { get; set; }
    }
}
