using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure
{
    public class DatabaseServer
    {
        public string Name { get; private set; }
        public string AdministratorLogin { get; private set; }
        public string Location { get; private set; }
        public Subscription Subscription { get; private set; }

        internal DatabaseServer(XElement xml, Subscription subscription)
        {
            xml.HydrateObject(XmlNamespaces.SqlAzure, this);
            Subscription = subscription;
        }

        public DatabaseServer(string administratorLogin, string location)
        {
            AdministratorLogin = administratorLogin;
            Location = location;
        }

        public async Task CreateAsync(Subscription subscription, string password)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(password));
            Contract.Requires(!string.IsNullOrWhiteSpace(AdministratorLogin));
            Contract.Requires(!string.IsNullOrWhiteSpace(Location));

            var ns = XmlNamespaces.SqlAzure;
            var content = new XElement(ns + "Server",
                new XElement(ns + "AdministratorLogin", AdministratorLogin),
                new XElement(ns + "AdministratorLoginPassword", password),
                new XElement(ns + "Location", Location)
                );

            var hc = GetRestClient(subscription);
            var response = await hc.PostAsync(content);
            var result = XElement.Parse (await response.Content.ReadAsStringAsync());
            if (result.Name != ns + "ServerName")
                throw new InvalidOperationException("Unexpected result creating database server: expected <ServerName>, got <" + result.Name + ">");
            Name = result.Value;

            Subscription = subscription;
        }

        public async Task DropAsync()
        {
            Contract.Requires(Subscription != null);
            await GetRestClient("/" + Name).DeleteAsync();
            Subscription = null;
        }

        AzureRestClient GetRestClient(string pathSuffix = null) { return GetRestClient(Subscription, pathSuffix); }

        AzureRestClient GetRestClient(Subscription subscription, string pathSuffix = null)
        {
            string servicePath = "servers";
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return subscription.GetDatabaseRestClient(servicePath);
        }

    }
}
