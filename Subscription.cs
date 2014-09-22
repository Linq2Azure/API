using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Linq;

using Linq2Azure.AffinityGroups;
using Linq2Azure.CloudServices;
using Linq2Azure.Locations;
using Linq2Azure.ReservedIps;
using Linq2Azure.SqlDatabases;
using Linq2Azure.StorageAccounts;
using Linq2Azure.TrafficManagement;

using System.Diagnostics;

namespace Linq2Azure
{
    /// <summary>
    /// This is the "root" type in the LinqToAzure API, from which all queries and operations are made.
    /// </summary>
    public class Subscription : IDisposable
    {
        public static readonly string CoreUri = "https://management.core.windows.net/";
        public static readonly string DatabaseUri = "https://management.core.windows.net:8443/";

        public IEnumerable<TraceListener> LogDestinations { get; set; }

        public Guid ID { get; private set; }
        public string Name { get; private set; }
        public X509Certificate2 ManagementCertificate { get; private set; }
        public LatentSequence<CloudService> CloudServices { get; private set; }
        public LatentSequence<DatabaseServer> DatabaseServers { get; private set; }
        public LatentSequence<TrafficManagerProfile> TrafficManagerProfiles { get; private set; }
        public LatentSequence<StorageAccount> StorageAccounts { get; private set; }
        public LatentSequence<AffinityGroup> AffinityGroups { get; private set; }
        public LatentSequence<Location> Locations { get; private set; }
        public LatentSequence<ReservedIp> ReservedIps { get; private set; }

        HttpClient _coreHttpClient20140601, _databaseHttpClient;

        public Subscription(Guid id, string name, X509Certificate2 managementCertificate)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(managementCertificate != null);

            ID = id;
            Name = name;
            ManagementCertificate = managementCertificate;

            Init();
        }

        public Subscription(string publishSettingsPath)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(publishSettingsPath));

            var pp = XDocument.Load(publishSettingsPath).Root.Element("PublishProfile");
            var sub = pp.Element("Subscription");

            ID = Guid.Parse((string)sub.Attribute("Id"));
            Name = (string)sub.Attribute("Name");
            var managementCertificateAttempt = (string)pp.Attribute("ManagementCertificate")
                ?? (string)sub.Attribute("ManagementCertificate");
            if (managementCertificateAttempt == null)
            {
                throw new ConfigurationErrorsException("Cannot find ManagementCertificate attribute neither in PublishProfile node, nor in Subscription in file " + publishSettingsPath);
            }
            ManagementCertificate = new X509Certificate2(Convert.FromBase64String(managementCertificateAttempt));

            Init();
        }

        void Init()
        {
            _coreHttpClient20140601 = AzureRestClient.CreateHttpClient(this, "2014-06-01", () => LogDestinations);
            _databaseHttpClient = AzureRestClient.CreateHttpClient(this, "2014-06-01", () => LogDestinations);

            CloudServices = new LatentSequence<CloudService>(GetCloudServicesAsync);
            DatabaseServers = new LatentSequence<DatabaseServer>(GetDatabaseServersAsync);
            TrafficManagerProfiles = new LatentSequence<TrafficManagerProfile>(GetTrafficManagerProfilesAsync);
            StorageAccounts = new LatentSequence<StorageAccount>(GetStorageAccountsAsync);
            AffinityGroups = new LatentSequence<AffinityGroup>(GetAffinityGroupsAsync);
            Locations = new LatentSequence<Location>(GetLocationsAsync);
            ReservedIps = new LatentSequence<ReservedIp>(GetReservedIpsAsync);
        }

        public Task CreateCloudServiceAsync(CloudService service) { return service.CreateAsync(this); }
        public Task CreateDatabaseServerAsync(DatabaseServer server, string adminPassword) { return server.CreateAsync(this, adminPassword); }
        public Task CreateTrafficManagerProfileAsync(TrafficManagerProfile profile) { return profile.CreateAsync(this); }
        public Task CreateStorageAccountAsync(StorageAccount storageAccount) { return storageAccount.CreateAsync(this); }
        public Task CreateAffinityGroupAsync(AffinityGroup affinityGroup) { return affinityGroup.CreateAsync(this); }
        public Task CreateReservedIpAsync(ReservedIp reservedIp) { return reservedIp.CreateAsync(this); }

        async Task<CloudService[]> GetCloudServicesAsync()
        {
            var xe = await GetCoreRestClient20140601("services/hostedServices").GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "HostedService").Select(x => new CloudService(x, this)).ToArray();
        }

        async Task<DatabaseServer[]> GetDatabaseServersAsync()
        {
            var xe = await GetDatabaseRestClient("services/sqlservers/servers").GetXmlAsync();
            return xe.Elements(XmlNamespaces.SqlAzure + "Server").Select(x => new DatabaseServer(x, this)).ToArray();
        }

        async Task<TrafficManagerProfile[]> GetTrafficManagerProfilesAsync()
        {
            var xe = await GetCoreRestClient20140601("services/WATM/profiles").GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "Profile").Select(x => new TrafficManagerProfile(x, this)).ToArray();
        }

        async Task<StorageAccount[]> GetStorageAccountsAsync()
        {
            var xe = await GetCoreRestClient20140601("services/storageservices").GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "StorageService").Select(x => new StorageAccount(x, this)).ToArray();
        }

        async Task<AffinityGroup[]> GetAffinityGroupsAsync()
        {
            var xe = await GetCoreRestClient20140601("affinitygroups").GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "AffinityGroup").Select(x => new AffinityGroup(x, this)).ToArray();
        }

        async Task<Location[]> GetLocationsAsync()
        {
            var xe = await GetCoreRestClient20140601("locations").GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "Location").Select(x => new Location(x, this)).ToArray();
        }

        async Task<ReservedIp[]> GetReservedIpsAsync()
        {
            var xe = await GetCoreRestClient20140601("services/networking/reservedips").GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "ReservedIP").Select(x => new ReservedIp(x, this)).ToArray();
        }

        internal AzureRestClient GetCoreRestClient20140601(string servicePath)
        {
            return new AzureRestClient(this, _coreHttpClient20140601, CoreUri, servicePath);
        }

        internal AzureRestClient GetDatabaseRestClient(string servicePath)
        {
            return new AzureRestClient(this, _databaseHttpClient, DatabaseUri, servicePath);
        }

        /// <summary>
        /// Implements http://msdn.microsoft.com/en-us/library/windowsazure/ee460783.aspx
        /// </summary>
        /// <param name="operationResponse">The response message from the operation for which we're awaiting completion.</param>
        internal async Task WaitForOperationCompletionAsync(HttpResponseMessage operationResponse)
        {
            var requestId = operationResponse.Headers.Single(h => h.Key == "x-ms-request-id").Value.Single();
            var delay = 1000;
            while (true)
            {
                var result = await GetOperationResultAsync(requestId);

                if (result == "Succeeded")
                    return;

                if (result != "InProgress")
                    throw new InvalidOperationException("Unknown error: result=" + result);

                await Task.Delay(delay);
                if (delay < 5000) delay += 1000;
            }
        }

        async Task<string> GetOperationResultAsync(string requestId)
        {
            var client = GetCoreRestClient20140601("operations/" + requestId);
            return ParseResult(await client.GetXmlAsync());
        }

        internal static string ParseResult(XElement result)
        {
            var error = result.Element(XmlNamespaces.WindowsAzure + "Error");
            if (error != null) AzureRestClient.Throw(null, error);

            return (string)result.Element(XmlNamespaces.WindowsAzure + "Status");
        }

        public void Dispose()
        {
            if (_coreHttpClient20140601 != null) _coreHttpClient20140601.Dispose();
            if (_databaseHttpClient != null) _databaseHttpClient.Dispose();
        }
    }
}
