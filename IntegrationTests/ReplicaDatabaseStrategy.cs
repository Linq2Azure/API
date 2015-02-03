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

            foreach (var server in await Subscription.DatabaseServers.AsTask())
            {
                await server.DropAsync();
            }

            await Subscription.CreateDatabaseServerAsync(DatabaseServer, Password);
            await Subscription.CreateDatabaseServerAsync(DestinationServer, Password);
            var databaseServer = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == DatabaseServer.Name);
            Database = await databaseServer.CreateDatabase(DatabaseRequest);
        }

        public async Task Teardown()
        {
            await DatabaseServer.DropAsync();
            await DestinationServer.DropAsync();
        }
    }
}