using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                using XmlReader reader = XmlReader.Create(new System.IO.StringReader(xml), settings);
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
                using XmlReader reader = XmlReader.Create(inStream, settings);
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
                using XmlReader reader = XmlReader.Create(filename, settings);
                base.Load(reader);
            }
        }

    }
}
