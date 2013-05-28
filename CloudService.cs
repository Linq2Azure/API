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
        static async Task Throw(HttpResponseMessage response)
        {
            string responseString = null;
            try { responseString = await response.Content.ReadAsStringAsync(); }
            catch { }

            string code = null, message = null;

            if (!string.IsNullOrWhiteSpace(responseString))
            {
                var ns = Constants.AzureXmlNamespace;
                var errorElement = XElement.Parse(responseString);
                code = (string)errorElement.Element(ns + "Code");
                message = (string)errorElement.Element(ns + "Message");
            }

            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(message))
                throw new InvalidOperationException("Error: " + response.StatusCode);
            else
                throw new InvalidOperationException(string.Join(" - ", new[] { code, message }));
        }

        internal static CloudService Load(XElement element, Subscription subscription)
        {
            Contract.Requires(element != null);
            Contract.Requires(subscription != null);

            var cs = new CloudService { Subscription = subscription };
            XNamespace ns = Constants.AzureXmlNamespace;

            cs.Url = new Uri((string)element.Element(ns + "Url"));
            cs.ServiceName = (string)element.Element(ns + "ServiceName");

            var properties = element.Element(ns + "HostedServiceProperties");
            cs.Description = (string)properties.Element(ns + "Description");
            cs.AffinityGroup = (string)properties.Element(ns + "AffinityGroup");
            cs.Label = ((string)properties.Element(ns + "Label")).FromBase64String();
            cs.Status = (string)properties.Element(ns + "Status");
            cs.DateCreated = (DateTime)properties.Element(ns + "DateCreated");
            cs.DateLastModified = (DateTime)properties.Element(ns + "DateLastModified");
            cs.Location = (string)properties.Element(ns + "Location");
            cs.ExtendedProperties = properties.Element(ns + "ExtendedProperties").Elements().ToDictionary(x => (string)x.Element(ns + "Name"), x => (string)x.Element(ns + "Value"));

            return cs;
        }

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

        public async Task Create(Subscription subscription)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(ServiceName));
            Contract.Requires(!string.IsNullOrWhiteSpace(Label));
            Contract.Requires(Location == null || Location.Trim().Length > 0);
            Contract.Requires(AffinityGroup == null || AffinityGroup.Trim().Length > 0);
            Contract.Requires((Location == null) != (AffinityGroup == null));

            var ns = Constants.AzureXmlNamespace;

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

            var hc = subscription.GetHttpClient();
            var payload = new StringContent(content.ToString());
            payload.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            var response = await hc.PostAsync("", payload);
            if (response.StatusCode != HttpStatusCode.Created) await Throw(response);
            Subscription = subscription;
        }

        public async Task Delete()
        {
            Contract.Requires(Subscription != null);
            var hc = Subscription.GetHttpClient(ServiceName);
            
            var response = await hc.DeleteAsync ("");            
            if (response.StatusCode != HttpStatusCode.OK) await Throw(response);
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
                                     + Subscription.SubscriptionId
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
                    return xdocument.Descendants(Constants.AzureXmlNamespace + "Deployment")
                        .Select(x => Deployment.Load(x, this));
                });
            }
        }


    }
}
