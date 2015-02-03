using System.Linq;
using System.Threading.Tasks;
using Linq2Azure;
using Linq2Azure.SqlDatabases;

namespace IntegrationTests
{
    public class StandardDatabaseStrategy : IScopedStrategy
    {
        private const string Password = "gj3eowl%5fi:edf";
        public Subscription Subscription;
        public DatabaseServer DatabaseServer;
        public DatabaseRequest DatabaseRequest;
        public Database Database;
        public string DatabaseName;

        public async Task Setup()
        {
            DatabaseName = "TestDB";
            DatabaseRequest = new DatabaseRequest(DatabaseName, Edition.Standard, PerformanceLevel.StandardS0, Collation.Default, 2.Gigabytes());
            Subscription =  TestConstants.Subscription;

            foreach (var server in await Subscription.DatabaseServers.AsTask())
            {
                await server.DropAsync();
            }

            DatabaseServer  = new DatabaseServer("testadmin", "West US");
            await Subscription.CreateDatabaseServerAsync(DatabaseServer, Password);
            var databaseServer = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == DatabaseServer.Name);
            Database = await databaseServer.CreateDatabase(DatabaseRequest);
        }

        public Task Teardown()
        {
            return DatabaseServer.DropAsync();
        }
    }
}