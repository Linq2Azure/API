using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Linq2Azure;
using Linq2Azure.SqlDatabases;

namespace IntegrationTests
{
    public class ReplicaDatabaseStrategy : IScopedStrategy
    {
        private const string Password = "gj3eowl%5fi:edf";
        public Subscription Subscription;
        public DatabaseServer DatabaseServer;
        public DatabaseServer DestinationServer;
        public DatabaseRequest DatabaseRequest;
        public Database Database;
        public string DatabaseName;

        public async Task Setup()
        {
            DatabaseName = "TestDB";
            DatabaseRequest = new DatabaseRequest(DatabaseName, Edition.Premium, PerformanceLevel.PremiumS1, Collation.Default, 20.Gigabytes());
            Subscription = TestConstants.Subscription;
            DatabaseServer = new DatabaseServer("testadmin", "West US");
            DestinationServer = new DatabaseServer("replicaadmin", "West US");

            var servers = await Subscription.DatabaseServers.AsTask();
            await Task.WhenAll(servers.Select(x => x.DropAsync()));

            await Subscription.CreateDatabaseServerAsync(DatabaseServer, Password);
            await Subscription.CreateDatabaseServerAsync(DestinationServer, Password);
            var databaseServer = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == DatabaseServer.Name);
            Database = await databaseServer.CreateDatabase(DatabaseRequest);
        }

        public async Task Teardown()
        {

            await Task.Run( async() =>
            {

                var servers = await DestinationServer.Databases.AsTask();
                var replicas = servers.ToList().Select(x => x.Replicas.AsTask());
                var allReplicas = await Task.WhenAll(replicas);
                var flattened = allReplicas.SelectMany(x => x);
                await Task.WhenAll(flattened.Select(x => x.Stop()));
                await DestinationServer.DropAsync();
                await DatabaseServer.DropAsync();
                return Unit.Instance;

            }).Catch();

        }
    }
}