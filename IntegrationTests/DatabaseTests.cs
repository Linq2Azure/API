using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Linq2Azure;
using Linq2Azure.SqlDatabases;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class DatabaseTests
    {
        public const string Password = "gj3eowl%5fi:edf";

        public readonly Subscription Subscription = TestConstants.Subscription;
        public readonly DatabaseServer DatabaseServer = new DatabaseServer("testadmin", "West US");
        public readonly string DatabaseName = "TestDB";

        [TestInitialize]
        public void Setup()
        {
            SetupImpl().Wait();
        }

        private async Task SetupImpl()
        {
            await Subscription.CreateDatabaseServerAsync(DatabaseServer, Password);
        }
      
        [TestMethod]
        public async Task CanCreateDatabases()
        {
            await DatabaseServer.CreateDatabase(DatabaseName, ServiceTier.StandardS0);

            var databaseServer = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == DatabaseServer.Name);
            var database = (await databaseServer.Databases.AsTask()).SingleOrDefault(x => x.Name == DatabaseName);

            Assert.IsNotNull(database);
        }

        [TestCleanup]
        public void Teardown()
        {
            TeardownImpl().Wait();
        }

        private async Task TeardownImpl()
        {
            await Task.Run(async () =>
            {
                if (DatabaseServer.Subscription == null)
                {
                    return;
                }

                try
                {
                    Debug.WriteLine("Dropping server...");
                    await DatabaseServer.DropAsync();
                    var deletedServer = (await Subscription.DatabaseServers.AsTask()).SingleOrDefault(d => d.Name == DatabaseServer.Name);
                    Assert.IsNull(deletedServer);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            });
        }
    }
}