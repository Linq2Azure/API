using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.ReservedIps
{
    public class ReservedIp
    {
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string Id { get; private set; }
        public string Label { get; private set; }
        public string State { get; private set; }
        public bool InUse { get; private set; }
        public string ServiceName { get; private set; }
        public string DeploymentName { get; private set; }
        public string Location { get; private set; }
        public Subscription Subscription { get; private set; }

        public ReservedIp(string name, string location)
            : this(name: name, label: name, location: location)
        {}

        public ReservedIp(string name, string label, string location)
        {
            Name = name;
            Label = label;
            Location = location;
        }

        internal ReservedIp(XElement xml, Subscription subscription)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);

            var azureNamespace = XmlNamespaces.WindowsAzure;

            xml.HydrateObject(azureNamespace, this);
            Subscription = subscription;
        }

        internal async Task CreateAsync(Subscription subscription)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(Name));
            Contract.Requires(!string.IsNullOrWhiteSpace(Label));
            Contract.Requires(!string.IsNullOrWhiteSpace(Location));

            var azureNamespace = XmlNamespaces.WindowsAzure;

            var content = new XElement(azureNamespace + "ReservedIP",
                new XElement(azureNamespace + "Name", Name),
                new XElement(azureNamespace + "Label", Label),
                string.IsNullOrWhiteSpace(Location) ? null : new XElement(azureNamespace + "Location", Location));

            var hc = subscription.GetCoreRestClient20140601("services/networking/reservedips");
            await hc.PostAsync(content);
            Subscription = subscription;
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Subscription != null);

            await GetRestClient("/" + Name).DeleteAsync();
            Subscription = null;
        }

        private AzureRestClient GetRestClient(string pathSuffix = null)
        {
            if (Subscription == null)
            {
                throw new InvalidOperationException("Subscription cannot be null for this operation.");
            }
            var servicePath = "services/networking/reservedips";
            if (!string.IsNullOrEmpty(pathSuffix))
            {
                servicePath += pathSuffix;
            }
            return Subscription.GetCoreRestClient20140601(servicePath);
        }
    }
}