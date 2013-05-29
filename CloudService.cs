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
        public Subscription Subscription { get; private set; }
        public IDictionary<string, string> ExtendedProperties { get; set; }
        public DateTime DateLastModified { get; private set; }
        public DateTime DateCreated { get; private set; }
        public string Status { get; private set; }
        public string Label { get; set; }
        public string AffinityGroup { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public Uri Url { get; private set; }
        public string ServiceName { get; set; }

        public CloudService()
        {
            ExtendedProperties = new Dictionary<string, string>();
        }

        internal CloudService (XElement element, Subscription subscription) : this()
        {
            Contract.Requires(element != null);
            Contract.Requires(subscription != null);

            Subscription = subscription;
            XNamespace ns = XmlNamespaces.Base;

            Url = new Uri((string)element.Element(ns + "Url"));
            ServiceName = (string)element.Element(ns + "ServiceName");

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
            Contract.Requires(!string.IsNullOrWhiteSpace(ServiceName));
            Contract.Requires(!string.IsNullOrWhiteSpace(Label));
            Contract.Requires(Location == null || Location.Trim().Length > 0);
            Contract.Requires(AffinityGroup == null || AffinityGroup.Trim().Length > 0);
            Contract.Requires((Location == null) != (AffinityGroup == null));

            var ns = XmlNamespaces.Base;

            var content = new XElement(ns + "CreateHostedService",
                new XElement(ns + "ServiceName", ServiceName),
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

        public async Task DeleteAsync()
        {
            Contract.Requires(Subscription != null);
            var hc = Subscription.GetRestClient("services/hostedservices/" + ServiceName);
            await hc.DeleteAsync();
            Subscription = null;
        }

        public IObservable<Deployment> Deployments
        {
            get { return GetDeployments().ToObservable().SelectMany(x => x); }
        }

        async Task<Deployment[]> GetDeployments()
        {
            var client = Subscription.GetRestClient("services/hostedservices/" + ServiceName + "?embed-detail=true");
            var results = await client.GetXmlAsync();
            return results.Descendants(XmlNamespaces.Base + "Deployments").Select(x => new Deployment(x, this)).ToArray();
        }
    }
}
