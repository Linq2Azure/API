using System;
using System.Linq;
using System.Threading.Tasks;
using Linq2Azure;
using Linq2Azure.SqlDatabases;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    public class ReplicaDatabaseSetup
    {
        private const string Password = "gj3eowl%5fi:edf";
        public Subscription Subscription;
        public DatabaseServer DatabaseServer;
        public DatabaseServer DestinationServer;
        public Database Database;
        public string DatabaseName;

        [TestInitialize]
        public void Setup()
        {
            SetupImpl().Wait();
        }

        private async Task SetupImpl()
        {
            DatabaseName = "TestDB" + Guid.NewGuid();
            Subscription = TestConstants.Subscription;
            DatabaseServer = new DatabaseServer("testadmin", "West US");
            DestinationServer = new DatabaseServer("replicaadmin", "West US");

            var servers = await Subscription.DatabaseServers.AsTask();
            await Task.WhenAll(servers.Select(x => x.DropAsync()));

            await Subscription.CreateDatabaseServerAsync(DatabaseServer, Password);
            await Subscription.CreateDatabaseServerAsync(DestinationServer, Password);
            var databaseServer = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == DatabaseServer.Name);
            Database = await databaseServer.CreateDatabase(DatabaseName,ServiceTier.PremiumS1);
            await Task.Delay(TimeSpan.FromSeconds(10));
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
                await DropReplicas(DestinationServer);
                await DropReplicas(DatabaseServer);
                await DestinationServer.DropAsync();
                await Task.Delay(TimeSpan.FromSeconds(10));
                await DatabaseServer.DropAsync();
                await Task.Delay(TimeSpan.FromSeconds(10));
                return Unit.Instance;
            }).Catch();
        }

        private async Task DropReplicas(DatabaseServer server)
        {
            var servers = await server.Databases.AsTask();
            var replicas = servers.ToList().Select(x => x.Replicas.AsTask());
            var allReplicas = await Task.WhenAll(replicas);
            var flattened = allReplicas.SelectMany(x => x);
            await Task.WhenAll(flattened.Select(x => x.SetForcedTerminationAllowed(true)));
            await Task.WhenAll(flattened.Select(x => x.Stop()));
        }
    }
}