using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.TrafficManagement
{
    public class TrafficManagerProfile
    {
        public string Name { get; private set; }
        public string DomainName { get; private set; }
        public bool Enabled { get; private set; }
        public Subscription Subscription { get; private set; }
        public LatentSequence<TrafficManagerDefinition> Definitions { get; private set; }

        private TrafficManagerProfile()
        {
            Definitions = new LatentSequence<TrafficManagerDefinition>(GetDefinitionsAsync);
            Enabled = true;
        }

        public TrafficManagerProfile(string name, string domainName) : this()
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(domainName));

            Name = name;
            DomainName = domainName;
        }

        internal TrafficManagerProfile(XElement xml, Subscription subscription) : this()
        {
            var ns = XmlNamespaces.WindowsAzure;
            xml.HydrateObject(ns, this);
            Enabled = (string) xml.Element(ns + "Status") != "Disabled";
            Subscription = subscription;
        }

        internal async Task CreateAsync(Subscription subscription)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(Name));
            Contract.Requires(!string.IsNullOrWhiteSpace(DomainName));

            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "ServiceTier",
                new XElement(ns + "DomainName", DomainName),
                new XElement(ns + "Name", Name)
                );

            var hc = GetRestClient(subscription);
            await hc.PostAsync(content);

            Subscription = subscription;
        }

        public Task AddDefinitionAsync(TrafficManagerDefinition definition)
        {
            return definition.CreateAsync(this);
        }

        [Obsolete("This method has been replaced by AddDefinitionAsync", false)]
        public Task AddDefinition(TrafficManagerDefinition definition)
        {
            return AddDefinitionAsync(definition);
        }

        public async Task UpdateAsync(bool enabled, string definitionVersion)
        {
            Contract.Requires(Subscription != null);

            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "ServiceTier",
                new XElement(ns + "Status", enabled ? "Enabled" : "Disabled"),
                new XElement(ns + "StatusDetails",
                    new XElement(ns + "EnabledVersion", definitionVersion))
                );

            var hc = GetRestClient("/" + Name);
            await hc.PutAsync(content);
            Enabled = enabled;
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Subscription != null);
            await GetRestClient("/" + Name).DeleteAsync();
            Subscription = null;
        }

        private async Task<TrafficManagerDefinition[]> GetDefinitionsAsync()
        {
            var xe = await GetRestClient("/" + Name + "/definitions").GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "Definition").Select(x => new TrafficManagerDefinition(x, this)).ToArray();
        }

        internal AzureRestClient GetRestClient(string pathSuffix = null)
        {
            return GetRestClient(Subscription, pathSuffix);
        }

        internal AzureRestClient GetRestClient(Subscription subscription, string pathSuffix = null)
        {
            if (subscription == null)
            {
                throw new InvalidOperationException("Subscription cannot be null for this operation.");
            }
            var servicePath = "services/WATM/profiles";
            if (!string.IsNullOrEmpty(pathSuffix))
            {
                servicePath += pathSuffix;
            }
            return subscription.GetCoreRestClient20140601(servicePath);
        }
    }
}