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
        public void ReplicationWorks()
        {
            Task.WaitAll(CanCreateLiveReplica(), CanCreateOfflineReplica(), CanCreateCopy());
        }

        private async Task CanCreateLiveReplica()
        {
            const string databaseName = "LiveReplicaTest";
            var database = await SoutheastAsiaDatabaseServer.CreateDatabase(databaseName, ServiceTier.PremiumS1);

            await database.CreateLiveReplica(AustraliaDatabaseServer);

            var replicaDatabase = (await AustraliaDatabaseServer.Databases.AsTask()).Single(x => x.Name == databaseName);
            var replica = (await replicaDatabase.Replicas.AsTask()).SingleOrDefault(x => x.DestinationDatabaseName == databaseName);
            Assert.IsNotNull(replica);
        }

        private async Task CanCreateCopy()
        {
            const string databaseName = "CopyTest";
            const string copyDatabaseName = "CopyTestCopy";
            var database = await SoutheastAsiaDatabaseServer.CreateDatabase(databaseName, ServiceTier.StandardS0);

            await database.CreateCopy(AustraliaDatabaseServer, copyDatabaseName);

            var databaseCopy = (await AustraliaDatabaseServer.Databases.AsTask()).SingleOrDefault(x => x.Name == copyDatabaseName);
            Assert.IsNotNull(databaseCopy);
        }

        private async Task CanCreateOfflineReplica()
        {
            const string databaseName = "OfflineReplicaTest";
            var database = await SoutheastAsiaDatabaseServer.CreateDatabase(databaseName, ServiceTier.PremiumS1);

            await database.CreateOfflineReplica(EastAsiaDatabaseServer);

            var replicaDatabase = (await EastAsiaDatabaseServer.Databases.AsTask()).Single(x => x.Name == databaseName);
            var replica = (await replicaDatabase.Replicas.AsTask()).SingleOrDefault(x => x.DestinationDatabaseName == databaseName);
            Assert.IsNotNull(replica);
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
                await Task.WhenAll(allReplicas.Select(x => x.SetForcedTerminationAllowed(true)));
                await Task.WhenAll(allReplicas.Select(x => x.Stop()));
            }
        }
    }
}
