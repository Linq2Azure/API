using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.ServiceBus
{
    /// <summary>
    /// Service Bus Namespace
    /// </summary>
    public class ServiceBusNamespace
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public string DefaultKey { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AcsManagementEndpoint { get; set; }
        public string ServiceBusEndpoint { get; set; }
        public string ConnectionString { get; set; }
        public string SubscriptionId { get; set; }
        public bool Enabled { get; set; }
        public bool Critical { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool CreateACSNamespace { get; set; }
        public bool CreateACSNamespaceSpecified { get; set; }
        public bool EventHubEnabled { get; set; }
        public string NamespaceType { get; set; }

        public Subscription Subscription { get; private set; }


        public ServiceBusNamespace(string name, string region)
        {
            Name = name;
            Region = region;
        }

        public ServiceBusNamespace(XElement element, Subscription subscription)
        {
            Contract.Requires(element != null);
            Contract.Requires(subscription != null);
            PopulatedSelf(subscription, element);
        }

        internal async Task CreateAsync(Subscription subscription)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(Name));
            Contract.Requires(!string.IsNullOrWhiteSpace(Region));

            var ns = XmlNamespaces.ServiceBusConfig;
            var content = new XElement(ns + "NamespaceDescription",
                new XElement(ns + "Name", Name),
                new XElement(ns + "Region", Region)
                );

            var hc = subscription.GetCoreRestClient20140601("/services/ServiceBus/Namespaces/");
            var response = await hc.PostAsync(content);
            var result = await response.Content.ReadAsStringAsync();
            Trace.TraceInformation("result recieved : {0}", result);
            var element = XElement.Parse(result);

            PopulatedSelf(subscription, element);
        }

        private void PopulatedSelf(Subscription subscription, XElement element)
        {
            element.HydrateObject(XmlNamespaces.ServiceBusConfig, this);
            CreatedAt = ((DateTime)element.Element(XmlNamespaces.ServiceBusConfig + "CreatedAt"));
            if (element.Element(XmlNamespaces.ServiceBusConfig + "UpdatedAt") != null)
            {
                UpdatedAt = ((DateTime)element.Element(XmlNamespaces.ServiceBusConfig + "UpdatedAt"));
            }
            else
            {
                UpdatedAt = null;
            }
            Subscription = subscription;
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Subscription != null);
            await Subscription.GetCoreRestClient20140601("/services/ServiceBus/Namespaces/" + Name).DeleteAsync();
            Subscription = null;
        }
    }
}