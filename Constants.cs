using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure
{
    class Constants
    {
        public static readonly XNamespace AzureXmlNamespace = "http://schemas.microsoft.com/windowsazure";
        public static readonly XNamespace AzureServiceConfigXmlNamespace = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration";
        public static readonly string ManagementBaseUri = "https://management.core.windows.net/";
    }
}
