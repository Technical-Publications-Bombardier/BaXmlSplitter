﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaXmlSplitter
{
    internal class ManualsPerKey
    {
        public string Name { get; set; }
        public string Manual { get; set; }
        public string DocNbr { get; set; }
        public string Cus { get; set; }
        public string Tsn { get; set; }
        public DateTime RevDate { get; set; }
        public DateTime ValidTime { get; set; }
        public int ObjectRef { get; set; }
        public int ObjectId { get; set; }
        public int ParentObjectId { get; set; }
        public string Key { get; set; }
    }
}
