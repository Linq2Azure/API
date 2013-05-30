using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure
{
    public class FirewallRule
    {
        public string Name { get; private set; }
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
            xml.HydrateObject(XmlNamespaces.SqlAzure, this);
        }

        public async Task CreateAsync(DatabaseServer server)
        {
            Contract.Requires(Parent == null);
            Contract.Requires(server != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(Name));
            Contract.Requires(!string.IsNullOrWhiteSpace(StartIpAddress));
            Contract.Requires(!string.IsNullOrWhiteSpace(EndIpAddress));

            var ns = XmlNamespaces.SqlAzure;
            var content = new XElement(ns + "FirewallRule",
                new XElement(ns + "StartIpAddress", StartIpAddress),
                new XElement(ns + "EndIpAddress", EndIpAddress)
                );

            var hc = GetRestClient(server);
            await hc.PutAsync(content);

            Parent = server;
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Parent != null);
            await GetRestClient().DeleteAsync();
            Parent = null;
        }

        AzureRestClient GetRestClient(string pathSuffix = null) { return GetRestClient(Parent, pathSuffix); }

        AzureRestClient GetRestClient(DatabaseServer server, string pathSuffix = null)
        {
            string servicePath = "servers/" + server.Name + "/firewallrules/" + Name;
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return server.Subscription.GetDatabaseRestClient(servicePath);
        }

    }
}
