using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.SqlDatabases
{
    public class Database
    {
        internal Database(XElement xml, DatabaseServer databaseServer)
        {
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
            DatabaseServer = databaseServer;
            Replicas = new LatentSequence<Replica>(GetReplicasAsync);
        }

        public DatabaseServer DatabaseServer { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string State { get; private set; }
        public Uri SelfLink { get; private set; }
        public Uri ParentLink { get; private set; }
        public int Id { get; private set; }
        public string Edition { get; private set; }
        public string CollationName { get; private set; }
        public DateTimeOffset CreationDate { get; private set; }
        public bool IsFederationRoot { get; private set; }
        public bool IsSystemObject { get; private set; }
        public decimal? SizeMB { get; private set; }
        public long MaxSizeBytes { get; private set; }
        public Guid ServiceObjectiveId { get; private set; }
        public Guid AssignedServiceObjectiveId { get; private set; }
        public int? ServiceObjectiveAssignmentState { get; private set; }
        public string ServiceObjectiveAssignmentStateDescription { get; private set; }
        public int? ServiceObjectiveAssignmentErrorCode { get; private set; }
        public string ServiceObjectiveAssignmentErrorDescription { get; private set; }
        public DateTimeOffset? ServiceObjectiveAssignmentSuccessDate { get; private set; }
        public DateTimeOffset? RecoveryPeriodStartDate { get; private set; }
        public bool IsSuspended { get; private set; }
        public LatentSequence<Replica> Replicas { get; private set; }

        private async Task<Replica[]> GetReplicasAsync()
        {
            Contract.Requires(DatabaseServer.Subscription != null);
            Contract.Requires(DatabaseServer.FullyQualifiedDomainName != null);
            Contract.Requires(Name != null);

            var restClient = GetRestClient();
            var response = await restClient.GetXmlAsync();

            return response.Elements(XmlNamespaces.WindowsAzure + "ServiceResource").Select(x => new Replica(x, this)).ToArray();
        }

        public Task<Replica> CreateCopy(DatabaseServer partnerServer, string partnerDatabase)
        {
            return CreateReplica(partnerServer, partnerDatabase, false, false);
        }

        public Task<Replica> CreateLiveReplica(DatabaseServer partnerServer)
        {
            return CreateReplica(partnerServer, Name, true, false);
        }

        public Task<Replica> CreateOfflineReplica(DatabaseServer partnerServer, string partnerDatabase)
        {
            return CreateReplica(partnerServer, partnerDatabase, true, true);
        }

        public async Task<Replica> CreateReplica(DatabaseServer partnerServer, string partnerDatabase, bool isContinuous, bool isOfflineSecondary)
        {
            Contract.Requires(DatabaseServer.Subscription != null);

            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "ServiceResource",
                new XElement(ns + "PartnerServer", partnerServer.Name),
                new XElement(ns + "PartnerDatabase", partnerDatabase),
                new XElement(ns + "IsContinuous", isContinuous),
                new XElement(ns + "IsOfflineSecondary", isOfflineSecondary)
                );

            var restClient = GetRestClient();

            Console.WriteLine(restClient.Uri);

            var response = await restClient.PostAsync(content);

            return new Replica(XElement.Parse(await response.Content.ReadAsStringAsync()), this);

        }
  
        public async Task<DatabaseServer> Delete()
        {
            var servicePath = "services/sqlservers/servers/" + DatabaseServer.Name + "/databases/" + Name;
            var client = DatabaseServer.Subscription.GetCoreRestClient20120301(servicePath);
            await client.DeleteAsync();
            return DatabaseServer;
        }

        private AzureRestClient GetRestClient()
        {
            var servicePath = "services/sqlservers/servers/" + DatabaseServer.Name + "/databases/" + Name + "/databasecopies";
            return DatabaseServer.Subscription.GetCoreRestClient20120301(servicePath);
        }

    }
}