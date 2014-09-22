using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.SqlDatabases
{
    public class FirewallRule
    {
        public string Name { get; private set; }
        public string State { get; private set; }
        public Uri SelfLink { get; private set; }
        public Uri ParentLink { get; private set; }
        public string StartIpAddress { get; private set; }
        public string EndIpAddress { get; private set; }
        public DatabaseServer Parent { get; private set; }

        public FirewallRule(string name, string startIpAddress, string endIpAddress)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(startIpAddress));
            Contract.Requires(!string.IsNullOrWhiteSpace(endIpAddress));

            Name = name;
            StartIpAddress = startIpAddress;
            EndIpAddress = endIpAddress;
        }

        internal FirewallRule(XElement xml, DatabaseServer parent)
        {
            Parent = parent;
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
            var startIpAddressElement = xml.Element(XmlNamespaces.WindowsAzure + "StartIPAddress");
            if (startIpAddressElement != null)
            {
                StartIpAddress = startIpAddressElement.Value;
            }
            var endIpAddressElement = xml.Element(XmlNamespaces.WindowsAzure + "EndIPAddress");
            if (endIpAddressElement != null)
            {
                EndIpAddress = endIpAddressElement.Value;
            }
        }

        internal async Task CreateAsync(DatabaseServer server)
        {
            Contract.Requires(Parent == null);
            Contract.Requires(server != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(Name));
            Contract.Requires(!string.IsNullOrWhiteSpace(StartIpAddress));
            Contract.Requires(!string.IsNullOrWhiteSpace(EndIpAddress));

            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "ServiceResource",
                new XElement(ns + "Name", Name),
                new XElement(ns + "StartIPAddress", StartIpAddress),
                new XElement(ns + "EndIPAddress", EndIpAddress)
                );

            var hc = GetRestClient(server);
            await hc.PostAsync(content);

            Parent = server;
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Parent != null);
            await GetRestClient("/" + Name).DeleteAsync();
            Parent = null;
        }

        AzureRestClient GetRestClient(string pathSuffix = null) { return GetRestClient(Parent, pathSuffix); }

        AzureRestClient GetRestClient(DatabaseServer server, string pathSuffix = null)
        {
            string servicePath = "services/sqlservers/servers/" + server.Name + "/firewallrules";
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return server.Subscription.GetDatabaseRestClient(servicePath);
        }
    }
}
