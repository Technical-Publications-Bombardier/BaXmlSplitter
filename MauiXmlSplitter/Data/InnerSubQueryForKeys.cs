using System.ComponentModel.DataAnnotations;

namespace MauiXmlSplitter.Data
{
    public class InnerSubQueryForKeys
    {
        [Required]
        public int ParentObjectId { get; set; }
        [Required]
        public string Key { get; set; }
    }
}
