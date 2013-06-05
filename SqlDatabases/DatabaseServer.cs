using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.SqlDatabases
{
    /// <summary>
    /// Allows management of SQL Azure servers. After creating a DatabaseServer and adding firewall rules, you can
    /// create databases by establishing an ADO.NET connection using the same credentials.
    /// </summary>
    public class DatabaseServer
    {
        public string Name { get; private set; }
        public string AdministratorLogin { get; private set; }
        public string Location { get; private set; }
        public Subscription Subscription { get; private set; }
        public LatentSequence<FirewallRule> FirewallRules { get; private set; }

        DatabaseServer()
        {
            FirewallRules = new LatentSequence<FirewallRule>(GetFirewallRulesAsync);
        }

        internal DatabaseServer(XElement xml, Subscription subscription) : this()
        {
            xml.HydrateObject(XmlNamespaces.SqlAzure, this);
            Subscription = subscription;
        }

        public DatabaseServer(string administratorLogin, string location) : this()
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(administratorLogin));
            Contract.Requires(!string.IsNullOrWhiteSpace(location));

            AdministratorLogin = administratorLogin;
            Location = location;
        }

        internal async Task CreateAsync(Subscription subscription, string adminPassword)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(adminPassword));
            Contract.Requires(!string.IsNullOrWhiteSpace(AdministratorLogin));
            Contract.Requires(!string.IsNullOrWhiteSpace(Location));

            var ns = XmlNamespaces.SqlAzure;
            var content = new XElement(ns + "Server",
                new XElement(ns + "AdministratorLogin", AdministratorLogin),
                new XElement(ns + "AdministratorLoginPassword", adminPassword),
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

        public Task AddFirewallRule(FirewallRule rule)
        {
            return rule.CreateAsync(this);
        }

        public async Task UpdateAdminPassword(string newAdminPassword)
        {
            Contract.Requires(Subscription != null);

            var ns = XmlNamespaces.SqlAzure;
            var content = new XElement(ns + "AdministratorLoginPassword", newAdminPassword);
            await GetRestClient("/" + Name + "?op=ResetPassword").PostAsync(content);
        }

        public async Task DropAsync()
        {
            Contract.Requires(Subscription != null);
            await GetRestClient("/" + Name).DeleteAsync();
            Subscription = null;
        }

        async Task<FirewallRule[]> GetFirewallRulesAsync()
        {
            XElement xe = await GetRestClient("/" + Name + "/firewallrules").GetXmlAsync();
            return xe.Elements(XmlNamespaces.SqlAzure + "FirewallRule").Select(x => new FirewallRule(x, this)).ToArray();
        }

        AzureRestClient GetRestClient(string pathSuffix = null) { return GetRestClient(Subscription, pathSuffix); }

        AzureRestClient GetRestClient(Subscription subscription, string pathSuffix = null)
        {
            if (subscription == null) throw new InvalidOperationException("Subscription cannot be null for this operation.");
            string servicePath = "servers";
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return subscription.GetDatabaseRestClient(servicePath);
        }

    }
}
