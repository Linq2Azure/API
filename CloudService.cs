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

            var hc = subscription.GetRestClient("hostedservices");
            await hc.PostAsync(content);
            Subscription = subscription;
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Subscription != null);
            var hc = Subscription.GetRestClient("hostedservices/" + ServiceName);
            await hc.DeleteAsync();
            Subscription = null;
        }

        public IObservable<Deployment> Deployments
        {
            get
            {
                if (Subscription == null) throw new InvalidOperationException();

                // URI variable.
                Uri requestUri = null;

                // Specify operation to use for the service management call.
                // This sample will use the operation for listing the hosted services.
                string operation = "hostedservices";

                // Create the request.
                requestUri = new Uri("https://management.core.windows.net/"
                                     + Subscription.SubscriptionID
                                     + "/services/"
                                     + operation + "/"
                                     + ServiceName
                                     + "?embed-detail=true");

                var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(requestUri);

                // Add the certificate to the request.
                httpWebRequest.ClientCertificates.Add(Subscription.ManagementCertificate);

                // Specify the version information in the header.
                httpWebRequest.Headers.Add("x-ms-version", "2012-03-01");
                return Observable.FromAsyncPattern<WebResponse>(httpWebRequest.BeginGetResponse, httpWebRequest.EndGetResponse)()
                .SelectMany(response =>
                {
                    // Make the call using the web request.
                    var httpWebResponse = (HttpWebResponse)response;

                    // TODO: handle other status codes?
                    // Display the web response status code.
                    //Console.WriteLine("Response status code: " + httpWebResponse.StatusCode);

                    string xmlResponse;

                    // Parse the web response.
                    using (var responseStream = httpWebResponse.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            xmlResponse = reader.ReadToEnd();
                            reader.Close();
                        }
                        // Close the resources no longer needed.
                        httpWebResponse.Close();
                    }

                    var xdocument = XDocument.Parse(xmlResponse);
                    return xdocument.Descendants(XmlNamespaces.Base + "Deployment")
                        .Select(x => new Deployment(x, this));
                });
            }
        }

    }
}
