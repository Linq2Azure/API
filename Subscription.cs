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
                from c in XDocument.Load(publishingSettingsPath).Root.Elements("PublishProfile")
                from s in c.Elements("Subscription")
                select new Subscription (
                    subscriptionID: Guid.Parse((string)s.Attribute("Id")),
                    subscriptionName: (string)s.Attribute("Name"),
                    managementCertificate: new X509Certificate2(Convert.FromBase64String((string)c.Attribute("ManagementCertificate")))
                    )
                ).Single();
        }

        public Guid ID { get; private set; }
        public string Name { get; private set; }
        public X509Certificate2 ManagementCertificate { get; private set; }
        
        readonly HttpClient _httpClient;

        public Subscription(Guid subscriptionID, string subscriptionName, X509Certificate2 managementCertificate)
        {
            ID = subscriptionID;
            Name = subscriptionName;
            ManagementCertificate = managementCertificate;

            _httpClient = AzureRestClient.CreateHttpClient(this);
        }

        public IObservable<CloudService> GetCloudServices()
        {
            return GetCloudServicesAsync().ToObservable().SelectMany(x => x);
        }

        public async Task<CloudService[]> GetCloudServicesAsync()
        {
            XElement xe = await GetRestClient("services/hostedServices").GetXmlAsync();
            return xe.Elements(XmlNamespaces.Base + "HostedService").Select(x => new CloudService(x, this)).ToArray();
        }

        internal AzureRestClient GetRestClient(string relativeUri)
        {
            return new AzureRestClient(this, _httpClient, relativeUri);
        }

        async Task<string> GetOperationResult(string requestId)
        {
            var client = GetRestClient("operations/" + requestId);
            var result = await client.GetXmlAsync();

            var error = result.Element("Error");
            if (error != null) AzureRestClient.Throw(null, error);

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
                    throw new InvalidOperationException("Unknown error: result=" + result);

                await Task.Delay(1000);
            }
        }
    }
}
