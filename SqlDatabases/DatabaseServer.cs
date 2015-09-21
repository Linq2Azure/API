using System;
using System.Diagnostics.Contracts;
using System.Linq;
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
        public string FullyQualifiedDomainName { get; private set; }
        public string Version { get; private set; }
        public string State { get; private set; }
        public Subscription Subscription { get; private set; }
        public LatentSequence<FirewallRule> FirewallRules { get; private set; }
        public LatentSequence<Database> Databases { get; private set; }

        DatabaseServer()
        {
            FirewallRules = new LatentSequence<FirewallRule>(GetFirewallRulesAsync);
            Databases = new LatentSequence<Database>(GetDatabasesAsync);
        }

        internal DatabaseServer(XElement xml, Subscription subscription) : this()
        {
            xml.HydrateObject(XmlNamespaces.SqlAzure, this);
            Subscription = subscription;
        }

        public DatabaseServer(string administratorLogin, string location, string version = "12.0") : this()
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(administratorLogin));
            Contract.Requires(!string.IsNullOrWhiteSpace(location));

            AdministratorLogin = administratorLogin;
            Location = location;
            Version = version;
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
                new XElement(ns + "Location", Location),
                new XElement(ns + "Version", Version)
                );

            var hc = GetRestClient(subscription);
            var response = await hc.PostAsync(content);
            var result = XElement.Parse (await response.Content.ReadAsStringAsync());
            if (result.Name != ns + "ServerName")
                throw new InvalidOperationException("Unexpected result creating database server: expected <ServerName>, got <" + result.Name + ">");
            Name = result.Value;

            Subscription = subscription;
        }

        async Task<Database[]> GetDatabasesAsync()
        {
            var xe = await Subscription.GetCoreRestClient20140601("services/sqlservers/servers/" + Name + "/databases?contentview=generic").GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "ServiceResource").Select(x => new Database(x, this)).ToArray();
        }

        [Obsolete("This method has been replaced by AddFirewallRuleAsync", false)]
        public Task AddFirewallRule(FirewallRule rule)
        {
            return AddFirewallRuleAsync(rule);
        }

        public Task AddFirewallRuleAsync(FirewallRule rule)
        {
            return rule.CreateAsync(this);
        }

        public Task AddWindowsAzureServicesFirewallRuleAsync()
        {
            return new FirewallRule("AllowAllWindowsAzureIps", "0.0.0.0", "0.0.0.0").CreateAsync(this);
        }

        [Obsolete("This method has been replaced by UpdateAdminPasswordAsync", false)]
        public Task UpdateAdminPassword(string newAdminPassword)
        {
            return UpdateAdminPasswordAsync(newAdminPassword);
        }

        public async Task UpdateAdminPasswordAsync(string newAdminPassword)
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

        public async Task<Database> CreateDatabase(string databaseName, ServiceTier serviceTier = ServiceTier.Basic, string collationName = "SQL_LATIN1_GENERAL_CP1_CI_AS", long maximumBytes = 2147483648)
        {
            Contract.Requires(Subscription != null);
            Contract.Requires(!String.IsNullOrEmpty(databaseName));

            var tier = new Tier(serviceTier);

            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "ServiceResource",
                new XElement(ns + "Name", databaseName),
                new XElement(ns + "Edition", tier.Edition.ToString()),
                new XElement(ns + "CollationName", collationName),
                new XElement(ns + "MaxSizeBytes", maximumBytes));

            if (tier.PerformanceLevel != Guid.Empty)
                content.Add(new XElement(ns + "ServiceObjectiveId", tier.PerformanceLevel));


            var response = await GetRestClient("/" + Name + "/databases").PostAsync(content);
            await Subscription.WaitForOperationCompletionAsync(response);
            return new Database(XElement.Parse(await response.Content.ReadAsStringAsync()), this);
        }

        async Task<FirewallRule[]> GetFirewallRulesAsync()
        {
            var xe = await GetRestClient("/" + Name + "/firewallrules").GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "ServiceResource").Select(x => new FirewallRule(x, this)).ToArray();
        }

        AzureRestClient GetRestClient(string pathSuffix = null) { return GetRestClient(Subscription, pathSuffix); }

        static AzureRestClient GetRestClient(Subscription subscription, string pathSuffix = null)
        {
            if (subscription == null) throw new InvalidOperationException("Subscription cannot be null for this operation.");
            var servicePath = "services/sqlservers/servers";
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return subscription.GetDatabaseRestClient(servicePath);
        }
    }
}
