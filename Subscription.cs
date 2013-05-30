﻿using System;
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
        public static readonly string CoreUri = "https://management.core.windows.net/";
        public static readonly string DatabaseUri = "https://management.database.windows.net:8443/";

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
        public LatentSequence<CloudService> CloudServices { get; private set; }
        public LatentSequence<DatabaseServer> DatabaseServers { get; private set; }
        
        readonly HttpClient _coreHttpClient, _databaseHttpClient;

        public Subscription(Guid subscriptionID, string subscriptionName, X509Certificate2 managementCertificate)
        {
            ID = subscriptionID;
            Name = subscriptionName;
            ManagementCertificate = managementCertificate;

            _coreHttpClient = AzureRestClient.CreateHttpClient(this, "2012-03-01");
            _databaseHttpClient = AzureRestClient.CreateHttpClient(this, "1.0");

            CloudServices = new LatentSequence<CloudService>(GetCloudServicesAsync);
            DatabaseServers = new LatentSequence<DatabaseServer>(GetDatabaseServersAsync);
        }

        async Task<CloudService[]> GetCloudServicesAsync()
        {
            XElement xe = await GetCoreRestClient("services/hostedServices").GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "HostedService").Select(x => new CloudService(x, this)).ToArray();
        }

        async Task<DatabaseServer[]> GetDatabaseServersAsync()
        {
            XElement xe = await GetDatabaseRestClient("servers").GetXmlAsync();
            return xe.Elements(XmlNamespaces.SqlAzure + "Server").Select(x => new DatabaseServer(x, this)).ToArray();
        }

        internal AzureRestClient GetCoreRestClient(string servicePath)
        {
            return new AzureRestClient(this, _coreHttpClient, CoreUri, servicePath);
        }

        internal AzureRestClient GetDatabaseRestClient(string servicePath)
        {
            return new AzureRestClient(this, _databaseHttpClient, DatabaseUri, servicePath);
        }

        async Task<string> GetOperationResultAsync(string requestId)
        {
            var client = GetCoreRestClient("operations/" + requestId);
            var result = await client.GetXmlAsync();

            var error = result.Element("Error");
            if (error != null) AzureRestClient.Throw(null, error);

            return (string) result.Element(XmlNamespaces.WindowsAzure + "Status");
        }

        internal async Task WaitForOperationCompletionAsync(HttpResponseMessage operationResponse)
        {
            var requestID = operationResponse.Headers.Single(h => h.Key == "x-ms-request-id").Value.Single();
            int delay = 1000;
            while (true)
            {
                var result = await GetOperationResultAsync(requestID);

                if (result == "Succeeded")
                    return;
                else if (result != "InProgress")
                    throw new InvalidOperationException("Unknown error: result=" + result);

                await Task.Delay(delay);
                if (delay < 5000) delay += 1000;
            }
        }
    }
}
