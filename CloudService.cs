using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Reactive.Threading.Tasks;

namespace Linq2Azure
{
    public class CloudService
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string AffinityGroup { get; set; }
        public IDictionary<string, string> ExtendedProperties { get; set; }
        public DateTime DateLastModified { get; private set; }
        public DateTime DateCreated { get; private set; }
        public string Status { get; private set; }
        public Uri Url { get; private set; }

        public Subscription Subscription { get; private set; }

        public CloudService(string serviceName, string locationOrAffinityGroup, bool isAffinityGroup = false)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(serviceName));
            Contract.Requires(!string.IsNullOrWhiteSpace(locationOrAffinityGroup));

            Name = Label = serviceName;

            if (isAffinityGroup)
                AffinityGroup = locationOrAffinityGroup;
            else
                Location = locationOrAffinityGroup;

            ExtendedProperties = new Dictionary<string, string>();
        }

        internal CloudService (XElement element, Subscription subscription) 
        {
            Contract.Requires(element != null);
            Contract.Requires(subscription != null);

            Subscription = subscription;
            PopulateFromXml(element);
        }

        void PopulateFromXml(XElement element)
        {
            XNamespace ns = XmlNamespaces.Base;

            Url = new Uri((string)element.Element(ns + "Url"));
            Name = (string)element.Element(ns + "ServiceName");

            var properties = element.Element(ns + "HostedServiceProperties");
            Description = (string)properties.Element(ns + "Description");
            AffinityGroup = (string)properties.Element(ns + "AffinityGroup");
            Label = ((string)properties.Element(ns + "Label")).FromBase64String();
            Status = (string)properties.Element(ns + "Status");
            DateCreated = (DateTime)properties.Element(ns + "DateCreated");
            DateLastModified = (DateTime)properties.Element(ns + "DateLastModified");
            Location = (string)properties.Element(ns + "Location");
            ExtendedProperties = properties.Element(ns + "ExtendedProperties").Elements().ToDictionary(x => (string)x.Element(ns + "Name"), x => (string)x.Element(ns + "Value"));
        }

        public async Task CreateAsync(Subscription subscription)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(Name));
            Contract.Requires(!string.IsNullOrWhiteSpace(Label));
            Contract.Requires(Location == null || Location.Trim().Length > 0);
            Contract.Requires(AffinityGroup == null || AffinityGroup.Trim().Length > 0);
            Contract.Requires((Location == null) != (AffinityGroup == null));

            var ns = XmlNamespaces.Base;
            var content = new XElement(ns + "CreateHostedService",
                new XElement(ns + "ServiceName", Name),
                new XElement(ns + "Label", Label.ToBase64String()),
                string.IsNullOrWhiteSpace(Description) ? null : new XElement(ns + "Description", Description),
                string.IsNullOrWhiteSpace(Location) ? null : new XElement(ns + "Location", Location),
                string.IsNullOrWhiteSpace(AffinityGroup) ? null : new XElement(ns + "AffinityGroup", AffinityGroup),
                ExtendedProperties == null || ExtendedProperties.Count == 0 ? null : 
                    new XElement(ns + "ExtendedProperties", ExtendedProperties.Select(kv =>
                        new XElement(ns + "ExtendedProperty",
                            new XElement(ns + "Name", kv.Key),
                            new XElement(ns + "Value", kv.Value))))
                            );

            var hc = subscription.GetRestClient("services/hostedservices");
            await hc.PostAsync(content);
            Subscription = subscription;
        }

        public async Task Refresh()
        {
            Contract.Requires(Subscription != null);
            XElement xe = await GetRestClient().GetXmlAsync();
            PopulateFromXml(xe);
        }

        public async Task SwapDeploymentsAsync()
        {
            Contract.Requires(Subscription != null);
            var deployments = await GetDeploymentsAsync();
            
            var production = deployments.SingleOrDefault(d => d.Slot == DeploymentSlot.Production);
            if (production == null) throw new InvalidOperationException("Cannot swap deployments: No production slot found");
            
            var staging = deployments.SingleOrDefault(d => d.Slot == DeploymentSlot.Staging);
            if (production == null) throw new InvalidOperationException("Cannot swap deployments: No staging slot found");

            var ns = XmlNamespaces.Base;
            var content = new XElement(ns + "Swap",
                new XElement(ns + "Production", production.Name),
                new XElement(ns + "SourceDeployment", staging.Name));

            var response = await GetRestClient().PostAsync(content);
            await Subscription.WaitForOperationCompletionAsync(response);
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Subscription != null);
            await GetRestClient().DeleteAsync();
            Subscription = null;
        }

        AzureRestClient GetRestClient(string queryString = null)
        {
            string uri = "services/hostedServices/" + Name;
            if (!string.IsNullOrEmpty(queryString)) uri += queryString;
            return Subscription.GetRestClient(uri);
        }

        public IObservable<Deployment> GetDeployments()
        {
            return GetDeploymentsAsync().ToObservable().SelectMany(x => x);
        }

        public async Task<Deployment> GetDeploymentAsync(DeploymentSlot slot)
        {
            return (await GetDeploymentsAsync()).SingleOrDefault(d => d.Slot == slot);
        }

        public async Task<Deployment[]> GetDeploymentsAsync()
        {
            var client = GetRestClient("?embed-detail=true");
            var results = await client.GetXmlAsync();
            return results.Element(XmlNamespaces.Base + "Deployments")
                .Elements(XmlNamespaces.Base + "Deployment")
                .Select(x => new Deployment(x, this))
                .ToArray();
        }
    }
}
