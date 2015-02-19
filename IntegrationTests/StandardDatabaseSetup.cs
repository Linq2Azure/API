using System.Linq;
using System.Threading.Tasks;
using Linq2Azure;
using Linq2Azure.SqlDatabases;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    public class StandardDatabaseSetup 
    {
        private const string Password = "gj3eowl%5fi:edf";
        public Subscription Subscription;
        public DatabaseServer DatabaseServer;
        public Database Database;
        public string DatabaseName;

        [TestInitialize]
        public void Setup()
        {
            SetupImpl().Wait();
        }

        private async Task SetupImpl()
        {
            DatabaseName = "TestDB";
            Subscription = TestConstants.Subscription;

            foreach (var server in await Subscription.DatabaseServers.AsTask())
            {
                await server.DropAsync();
            }

            DatabaseServer = new DatabaseServer("testadmin", "West US");
            await Subscription.CreateDatabaseServerAsync(DatabaseServer, Password);
            var databaseServer = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == DatabaseServer.Name);
            Database = await databaseServer.CreateDatabase(DatabaseName,ServiceTier.StandardS0);
        }

        [TestCleanup]
        public void Teardown()
        {
            DatabaseServer.DropAsync().Wait();
        }
    }
}