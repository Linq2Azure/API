using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using System.Threading;

namespace Linq2Azure
{
    public class Subscription
    {
        public static Subscription FromPublisherSettingsPath(string publishingSettingsPath)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(publishingSettingsPath));

            return (
                from c in XDocument.Load(publishingSettingsPath).Descendants("PublishProfile")
                from s in c.Descendants("Subscription")
                select new Subscription
                {
                    SubscriptionId = Guid.Parse((string)s.Attribute("Id")),
                    SubscriptionName = (string)s.Attribute("Name"),
                    ManagementCertificate =
                        new X509Certificate2(
                        Convert.FromBase64String((string)c.Attribute("ManagementCertificate"))),
                }).Single();
        }

        public X509Certificate2 ManagementCertificate { get; set; }
        public Guid SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }

        public IObservable<CloudService> CloudServices
        {
            get { return GetCloudServices().ToObservable().SelectMany(x => x); }
        }

        async Task<CloudService[]> GetCloudServices()
        {
            var hc = GetHttpClient();
            string xmlResponse = await hc.GetStringAsync("");
            var xe = XElement.Parse(xmlResponse);
            return xe.Descendants(Constants.AzureXmlNamespace + "HostedService")
                .Select(x => CloudService.Load(x, this)).ToArray();
        }

        internal HttpClient GetHttpClient(string relativeUri = null)
        {
            string uriString = Constants.ManagementBaseUri + SubscriptionId + "/services/hostedservices";
            if (!string.IsNullOrWhiteSpace(relativeUri)) uriString += "/" + relativeUri;
            Uri requestUri = new Uri(uriString);
            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(ManagementCertificate);
            var logger = new LoggingHandler(handler);
            var hc = new HttpClient(logger, true) { BaseAddress = requestUri };
            hc.DefaultRequestHeaders.Add("x-ms-version", "2012-03-01");
            return hc;
        }

        class LoggingHandler : DelegatingHandler
        {
            public LoggingHandler(HttpMessageHandler nextHandler)
            {
                InnerHandler = nextHandler;
            }

            protected async override Task<HttpResponseMessage> SendAsync
              (HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Console.WriteLine("Requesting: " + request.RequestUri);
                var response = await base.SendAsync(request, cancellationToken);
                Console.WriteLine("Got response: " + response.StatusCode);
                return response;
            }
        }
    }
}
