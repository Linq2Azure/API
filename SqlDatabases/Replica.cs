using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.SqlDatabases
{
    public class Replica
    {

        internal Replica(XElement xml, Database database)
        {
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
            Database = database;
        }

        public Database Database { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string State { get; private set; }
        public Uri SelfLink { get; private set; }
        public Uri ParentLink { get; private set; }
        public string SourceServerName { get; private set; }
        public string SourceDatabaseName { get; private set; }
        public string DestinationServerName { get; private set; }
        public string DestinationDatabaseName { get; private set; }
        public bool IsContinuous { get; private set; }
        public bool IsOfflineSecondary { get; private set; }
        public bool IsTerminationAllowed { get; private set; }
        public DateTimeOffset StartDate { get; private set; }
        public DateTimeOffset ModifyDate { get; private set; }
        public decimal PercentComplete { get; private set; }
        public int ReplicationState { get; private set; }
        public string ReplicationStateDescription { get; private set; }
        public string LocalDatabaseId { get; private set; }
        public bool IsLocalDatabaseReplicationTarget { get; private set; }
        public bool IsInterlinkConnected { get; private set; }

        public async Task Stop(bool isForcedTerminate)
        {
            var client = GetRestClient(Database.DatabaseServer, Database.Name);

            await SendIsForcedTerminateUpdate(client, isForcedTerminate);
            await SendDelete(client);
        }

        private async Task SendIsForcedTerminateUpdate(AzureRestClient client, bool isForcedTerminate)
        {
            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "ServiceResource",
                new XElement(ns + "IsForcedTerminate", isForcedTerminate)
                );

            var response = await client.PutAsync(content);
            await Database.DatabaseServer.Subscription.WaitForOperationCompletionAsync(response);
        }

        private async Task SendDelete(AzureRestClient client)
        {
            var response = await client.DeleteAsync();
            await Database.DatabaseServer.Subscription.WaitForOperationCompletionAsync(response);
        }

        private AzureRestClient GetRestClient(DatabaseServer server, string databaseName)
        {
            var servicePath = "services/sqlservers/servers/" + server.Name + "/databases/" + databaseName + "/databasecopies/" + Name;
            return server.Subscription.GetDatabaseRestClient(servicePath);
        }
    }
}