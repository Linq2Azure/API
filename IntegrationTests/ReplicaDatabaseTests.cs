using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Linq2Azure;
using Linq2Azure.SqlDatabases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace IntegrationTests
{
    [TestClass]
    public class ReplicaDatabaseTests  
    {
        private const string Password = "gj3eowl%5fi:edf";
        public readonly Subscription Subscription = TestConstants.Subscription;

        public readonly DatabaseServer SoutheastAsiaDatabaseServer = new DatabaseServer("testadmin", "Southeast Asia");
        public readonly DatabaseServer EastAsiaDatabaseServer = new DatabaseServer("testadmin", "East Asia");
        public readonly DatabaseServer AustraliaDatabaseServer = new DatabaseServer("testadmin", "Australia East");

        [TestInitialize]
        public void Setup()
        {
            try
            {
                SetupImpl().Wait();
            }
            catch (Exception e)
            {
                TeardownImpl().Wait();
                throw;
            }
        }

        private async Task SetupImpl()
        {
            await Subscription.CreateDatabaseServerAsync(SoutheastAsiaDatabaseServer, Password);
            await Subscription.CreateDatabaseServerAsync(EastAsiaDatabaseServer, Password);
            await Subscription.CreateDatabaseServerAsync(AustraliaDatabaseServer, Password);
        }

        [TestMethod]
        public async Task CanCreateLiveReplica()
        {
            const string databaseName = "LiveReplicaTest";
            var database = await SoutheastAsiaDatabaseServer.CreateDatabase(databaseName, ServiceTier.PremiumS1);

            await database.CreateLiveReplica(AustraliaDatabaseServer);

            var replicaDatabase = (await AustraliaDatabaseServer.Databases.AsTask()).Single(x => x.Name == databaseName);
            var replica = (await replicaDatabase.Replicas.AsTask()).SingleOrDefault(x => x.DestinationDatabaseName == databaseName);
            Assert.IsNotNull(replica);
        }

        [TestMethod]
        public async Task CanCreateCopy()
        {
            const string databaseName = "CopyTest";
            const string copyDatabaseName = "CopyTestCopy";
            var database = await SoutheastAsiaDatabaseServer.CreateDatabase(databaseName, ServiceTier.StandardS0);

            await database.CreateCopy(AustraliaDatabaseServer, copyDatabaseName);

            var databaseCopy = (await AustraliaDatabaseServer.Databases.AsTask()).SingleOrDefault(x => x.Name == copyDatabaseName);
            Assert.IsNotNull(databaseCopy);
        }

        [TestMethod]
        public async Task CanCreateOfflineReplica()
        {
            const string databaseName = "OfflineReplicaTest";
            var database = await SoutheastAsiaDatabaseServer.CreateDatabase(databaseName, ServiceTier.PremiumS1);

            await database.CreateOfflineReplica(EastAsiaDatabaseServer);

            var replicaDatabase = (await EastAsiaDatabaseServer.Databases.AsTask()).Single(x => x.Name == databaseName);
            var replica = (await replicaDatabase.Replicas.AsTask()).SingleOrDefault(x => x.DestinationDatabaseName == databaseName);
            Assert.IsNotNull(replica);
        }

        [TestMethod]
        public async Task CanTerminateReplicationOnPrimaryDatabaseIfNotForced()
        {
            const string databaseName = "TerminateReplicationPrimaryDatabaseNotForced";
            var database = await SoutheastAsiaDatabaseServer.CreateDatabase(databaseName, ServiceTier.PremiumS1);

            await database.CreateLiveReplica(AustraliaDatabaseServer);

            var replica = (await database.Replicas.AsTask()).SingleOrDefault();
            Assert.IsNotNull(replica);

            Thread.Sleep(120000);
            await replica.Stop(false);

            var replicaNew = (await database.Replicas.AsTask()).SingleOrDefault();
            Assert.IsNull(replicaNew);
        }

        [TestMethod]
        public async Task CanNotTerminateReplicationOnSecondaryDatabaseIfNotForced()
        {
            const string databaseName = "TerminateReplicationSecondaryDatabaseNotForced";
            var database = await SoutheastAsiaDatabaseServer.CreateDatabase(databaseName, ServiceTier.PremiumS1);

            await database.CreateLiveReplica(AustraliaDatabaseServer);
            var sourceReplica = (await database.Replicas.AsTask()).SingleOrDefault(x => x.DestinationDatabaseName == databaseName);
            Assert.IsNotNull(sourceReplica);

            var replicaDatabase = (await AustraliaDatabaseServer.Databases.AsTask()).Single(x => x.Name == databaseName);
            var destReplica = (await replicaDatabase.Replicas.AsTask()).SingleOrDefault(x => x.DestinationDatabaseName == databaseName);
            Assert.IsNotNull(destReplica);

            var throwsAzureRestException = false;
            try
            {
                await destReplica.Stop(false);
                Assert.Fail("AzureRestException expected to be thrown");
            }
            catch (AzureRestException e)
            {
                throwsAzureRestException = true;
            }

            Assert.IsTrue(throwsAzureRestException);
        }

        [TestMethod]
        public async Task CanTerminateReplicationOnPrimaryDatabaseIfForced()
        {
            const string databaseName = "TerminateReplicationPrimaryDatabaseForced";
            var database = await SoutheastAsiaDatabaseServer.CreateDatabase(databaseName, ServiceTier.PremiumS1);

            await database.CreateLiveReplica(AustraliaDatabaseServer);

            var replica = (await database.Replicas.AsTask()).SingleOrDefault();
            Assert.IsNotNull(replica);

            await replica.Stop(true);

            var replicaNew = (await database.Replicas.AsTask()).SingleOrDefault();
            Assert.IsNull(replicaNew);
        }

        [TestMethod]
        public async Task CanTerminateReplicationOnSecondaryDatabaseIfForced()
        {
            const string databaseName = "TerminateReplicationSecondaryDatabaseForced";
            var database = await SoutheastAsiaDatabaseServer.CreateDatabase(databaseName, ServiceTier.PremiumS1);

            await database.CreateLiveReplica(AustraliaDatabaseServer);
            var sourceReplica = (await database.Replicas.AsTask()).SingleOrDefault(x => x.DestinationDatabaseName == databaseName);
            Assert.IsNotNull(sourceReplica);

            var replicaDatabase = (await AustraliaDatabaseServer.Databases.AsTask()).Single(x => x.Name == databaseName);
            var destReplica = (await replicaDatabase.Replicas.AsTask()).SingleOrDefault(x => x.DestinationDatabaseName == databaseName);
            Assert.IsNotNull(destReplica);

            await destReplica.Stop(true);

            var replicaNew = (await database.Replicas.AsTask()).SingleOrDefault();
            Assert.IsNull(replicaNew);
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
                try
                {
                    await DropReplicas(SoutheastAsiaDatabaseServer);
                    await DropReplicas(EastAsiaDatabaseServer);
                    await DropReplicas(AustraliaDatabaseServer);

                    await SoutheastAsiaDatabaseServer.DropAsync();
                    await EastAsiaDatabaseServer.DropAsync();
                    await AustraliaDatabaseServer.DropAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            });
        }

        private async Task DropReplicas(DatabaseServer server)
        {
            var databases = await server.Databases.AsTask();
            var allReplicas = databases.SelectMany(d => d.Replicas.AsArray()).Where(r => r.IsContinuous).ToArray();
            if (allReplicas.Any())
            {
                await Task.WhenAll(allReplicas.Select(x => x.Stop(true)));
            }
        }
    }
}
