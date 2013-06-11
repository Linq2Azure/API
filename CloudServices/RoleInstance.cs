using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.CloudServices
{
    /// <summary>
    /// Read-only view of RoleInstance information.
    /// </summary>
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

        public Deployment Parent { get; private set; }

        internal RoleInstance(XElement element, Deployment parent)
        {
            element.HydrateObject(XmlNamespaces.WindowsAzure, this);
            Parent = parent;
        }

        public async Task RebootAsync()
        {
            Contract.Requires(Parent != null);

            HttpResponseMessage response = await GetRestClient("?comp=reboot").PostAsync();
            await Parent.Parent.Subscription.WaitForOperationCompletionAsync(response);
        }

        AzureRestClient GetRestClient(string pathSuffix = null)
        {
            string servicePath = "/roleinstances/" + InstanceName;
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return Parent.GetRestClient(servicePath);
        }
    }
}
