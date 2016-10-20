using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;       // XmlNamespace and others
using System.Xml.Linq;  // LINQ for XML
using System.Xml.XPath; // XPath extensions
using System.IO;

namespace PsoftJournalVoucher
{
    public class XMLToString
    {

        public string GetXMLAsString(XmlDocument myxml)
        {
            return myxml.OuterXml;
        }
    }
}