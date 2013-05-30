using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure
{
    class XmlNamespaces
    {
        public static readonly XNamespace WindowsAzure = "http://schemas.microsoft.com/windowsazure";
        public static readonly XNamespace SqlAzure = "http://schemas.microsoft.com/sqlazure/2010/12/";
        public static readonly XNamespace ServiceConfig = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration";
    }
}
