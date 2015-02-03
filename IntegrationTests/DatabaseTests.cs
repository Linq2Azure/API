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
    public class DatabaseTests : IDisposable
    {
        public const string Password = "gj3eowl%5fi:edf";

        public readonly Subscription Subscription = TestConstants.Subscription;
        public readonly DatabaseServer DatabaseServer = new DatabaseServer("testadmin", "West US");
        public readonly DatabaseServer ReplicaServer = new DatabaseServer("testadminreplica", "West US");

        public DatabaseTests()
        {
            Debug.WriteLine("Creating database server...");
            Subscription.CreateDatabaseServerAsync(DatabaseServer, Password).Wait();

            Debug.WriteLine("Creating replica server...");
            Subscription.CreateDatabaseServerAsync(ReplicaServer, Password).Wait();
        }

        [TestMethod]
        public async Task CanGetDatabases()
        {
            var database = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == DatabaseServer.Name);

            Assert.AreNotEqual(0, database.Databases.AsArray().Length);
        }

        [TestMethod]
        public async Task CreateDatabaseTest()
        {
            await Subscription.CreateDatabaseServerAsync(DatabaseServer, Password);
            var databaseServer = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == DatabaseServer.Name);
            var databaseRequest = new DatabaseRequest("TestDB", Edition.Standard, PerformanceLevel.StandardS0, Collation.Default, 2.Gigabytes());
            var database = await databaseServer.CreateDatabase(databaseRequest);
            Scoped.Execute(() => Assert.AreEqual(2,databaseServer.Databases.AsArray().Length),async () => await database.Delete());
        }

        [TestMethod]
        public async Task CreateDatabaseActiveReplicaTest()
        {

            const string dbName = "PremiumTest";

            await Subscription.CreateDatabaseServerAsync(DatabaseServer, Password);
            await Subscription.CreateDatabaseServerAsync(ReplicaServer, Password);

            var databaseServer = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == DatabaseServer.Name);
            var replicaServer = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == ReplicaServer.Name);

            var databaseRequest = new DatabaseRequest(dbName, Edition.Premium, PerformanceLevel.PremiumS1,Collation.Default, 2.Gigabytes());

            var database = await databaseServer.CreateDatabase(databaseRequest);
            var replica = await database.CreateLiveReplica(replicaServer);

            Scoped.Execute(() =>
                Assert.AreEqual(1, databaseServer.Databases.AsArray().SingleOrDefault(x => x.Name == dbName).Replicas.AsArray().Length),
                async () =>
                {
                    await database.Delete();
                });
        }

        public void Dispose()
        {
            if (DatabaseServer.Subscription == null && ReplicaServer.Subscription == null)
            {
                return;
            }

            Debug.WriteLine("Dropping server...");
            DatabaseServer.DropAsync().Wait();
            Debug.WriteLine("Dropping replica...");
            ReplicaServer.DropAsync().Wait();

            // Verify the deletion only if we're testing the database server itself.
            if (GetType() == typeof(DatabaseServerTests))
            {
                VerifyDeletion();
            }
        }

        private void VerifyDeletion()
        {
            Debug.WriteLine("Verifying database server deletion...");
            Assert.IsNull(Subscription.DatabaseServers.AsArray().SingleOrDefault(d => d.Name == DatabaseServer.Name));
        }

    }
}