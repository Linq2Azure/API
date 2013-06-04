using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.CloudServices
{
    public class RoleInstance
    {
        public string RoleName { get; private set; }
        public string InstanceName { get; private set; }
        public string InstanceStatus { get; private set; }
        public string InstanceUpgradeDomain { get; private set; }
        public string InstanceFaultDomain { get; private set; }
        public string InstanceSize { get; private set; }
        public string InstanceStateDetails { get; private set; }
        public string InstanceErrorCode { get; private set; }
        public string IpAddress { get; private set; }
        public string PowerState { get; private set; }
        public string HostName { get; private set; }
        public string RemoteAccessCertificateThumbprint { get; private set; }

        public RoleInstance(XElement element)
        {
            element.HydrateObject(XmlNamespaces.WindowsAzure, this);
        }
    }
}
