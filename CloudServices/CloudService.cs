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

namespace Linq2Azure.CloudServies
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
        public DeploymentSet Deployments { get; private set; }
        public LatentSequence<ServiceCertificate> Certificates { get; private set; }
        public Subscription Subscription { get; private set; }

        CloudService()
        {
            Deployments = new DeploymentSet(GetDeploymentsAsync);
            Certificates = new LatentSequence<ServiceCertificate>(GetCertificatesAsync);
        }

        public CloudService(string serviceName, string locationOrAffinityGroup, bool isAffinityGroup = false) : this()
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

        internal CloudService(XElement element, Subscription subscription) : this()
        {
            Contract.Requires(element != null);
            Contract.Requires(subscription != null);

            Subscription = subscription;
            PopulateFromXml(element);
        }

        void PopulateFromXml(XElement element)
        {
            XNamespace ns = XmlNamespaces.WindowsAzure;

            Url = new Uri((string)element.Element(ns + "Url"));
            Name = (string)element.Element(ns + "ServiceName");

            var properties = element.Element(ns + "HostedServiceProperties");
            properties.HydrateObject(ns, this);
            if (!string.IsNullOrEmpty (Label)) Label = Label.FromBase64String();
            DateCreated = (DateTime)properties.Element(ns + "DateCreated");
            DateLastModified = (DateTime)properties.Element(ns + "DateLastModified");
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

            var ns = XmlNamespaces.WindowsAzure;
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

            var hc = subscription.GetCoreRestClient("services/hostedservices");
            await hc.PostAsync(content);
            Subscription = subscription;
        }

        public async Task RefreshAsync()
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

            var ns = XmlNamespaces.WindowsAzure;
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

        AzureRestClient GetRestClient(string pathSuffix = null)
        {
            if (Subscription == null) throw new InvalidOperationException("Subscription cannot be null for this operation.");
            string servicePath = "services/hostedServices/" + Name;
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return Subscription.GetCoreRestClient(servicePath);
        }

        async Task<Deployment[]> GetDeploymentsAsync()
        {
            var client = GetRestClient("?embed-detail=true");
            var results = await client.GetXmlAsync();
            return results.Element(XmlNamespaces.WindowsAzure + "Deployments")
                .Elements(XmlNamespaces.WindowsAzure + "Deployment")
                .Select(x => new Deployment(x, this))
                .ToArray();
        }

        async Task<ServiceCertificate[]> GetCertificatesAsync()
        {
            var client = GetRestClient("/certificates");
            var results = await client.GetXmlAsync();
            return results.Elements(XmlNamespaces.WindowsAzure + "Certificate")
                .Select(x => new ServiceCertificate(x, this))
                .ToArray();
        }
    }

    public class DeploymentSet : LatentSequence<Deployment>
    {
        public DeploymentSet(Func<Task<Deployment[]>> taskGenerator) : base (taskGenerator)
        {
        }

        public async Task<Deployment> GetProductionAsync()
        {
            return (await AsTask()).SingleOrDefault(d => d.Slot == DeploymentSlot.Production);
        }

        public async Task<Deployment> GetStagingAsync()
        {
            return (await AsTask()).SingleOrDefault(d => d.Slot == DeploymentSlot.Staging);
        }
    }
}
