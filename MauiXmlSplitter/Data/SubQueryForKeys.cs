using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaXmlSplitter
{
    internal class SubQueryForKeys
    {
        [Required]
        public int ObjectId { get; set; }
        [Required]
        public string Key { get; set; }
    }
}
