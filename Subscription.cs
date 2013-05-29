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
                    SubscriptionID = Guid.Parse((string)s.Attribute("Id")),
                    SubscriptionName = (string)s.Attribute("Name"),
                    ManagementCertificate =
                        new X509Certificate2(
                        Convert.FromBase64String((string)c.Attribute("ManagementCertificate"))),
                }).Single();
        }

        public X509Certificate2 ManagementCertificate { get; set; }
        public Guid SubscriptionID { get; set; }
        public string SubscriptionName { get; set; }

        public IObservable<CloudService> CloudServices
        {
            get { return GetCloudServices().ToObservable().SelectMany(x => x); }
        }

        internal AzureRestClient GetRestClient(string relativeUri)
        {
            return new AzureRestClient(this, relativeUri);
        }

        async Task<CloudService[]> GetCloudServices()
        {
            XElement xe = await GetRestClient("services/hostedServices").GetXmlAsync();
            return xe.Descendants(XmlNamespaces.Base + "HostedService").Select(x => new CloudService(x, this)).ToArray();
        }

        async Task<string> GetOperationResult(string requestId)
        {
            var client = GetRestClient("operations/" + requestId);
            var result = await client.GetXmlAsync();

            var error = result.Element("Error");
            if (error != null)
            {
                var httpStatus = (HttpStatusCode)(int)result.Element("HttpStatusCode");
                AzureRestClient.Throw(httpStatus, error);
            }

            return (string) result.Element(XmlNamespaces.Base + "Status");
        }

        internal async Task WaitForOperationCompletionAsync(HttpResponseMessage operationResponse)
        {
            var requestID = operationResponse.Headers.Single(h => h.Key == "x-ms-request-id").Value.Single();
            while (true)
            {
                var result = await GetOperationResult(requestID);

                if (result == "Succeeded")
                    return;
                else if (result != "InProgress")
                    throw new InvalidOperationException("Unknown error");

                await Task.Delay(1000);
            }
        }
    }
}
