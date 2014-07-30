using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linq2Azure;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Linq2Azure.SqlDatabases;

namespace IntegrationTests
{
    [TestClass]
    public class DatabaseServerTests : IDisposable
    {
        public const string Password = "gj3eowl%5fi:edf";

        public readonly Subscription Subscription = TestConstants.Subscription;
        public readonly DatabaseServer DatabaseServer = new DatabaseServer("testadmin", "West US");

        public DatabaseServerTests()
        {
            Debug.WriteLine("Creating database server...");
            Subscription.CreateDatabaseServerAsync (DatabaseServer, Password).Wait();
        }

        [TestMethod]
        public async Task CanUseDatabaseServer()
        {
            await CanCreateDatabaseServer();
            await CanUpdateDatabaseServerPassword();
        }

        async Task CanCreateDatabaseServer()
        {
            Assert.IsNotNull(DatabaseServer.Name);
            Assert.IsNotNull(DatabaseServer.Subscription);
            Debug.WriteLine("Retrieving database server...");
            Assert.IsNotNull ((await Subscription.DatabaseServers.AsTask()).SingleOrDefault (d => d.Name == DatabaseServer.Name));
        }

        async Task CanUpdateDatabaseServerPassword()
        {
            Debug.WriteLine("Updating password...");
            await DatabaseServer.UpdateAdminPasswordAsync(Password + "foo");
        }

        public void Dispose()
        {
            if (DatabaseServer.Subscription == null) return;
            Debug.WriteLine("Dropping server...");
            DatabaseServer.DropAsync().Wait();

            // Verify the deletion only if we're testing the database server itself.
            if (GetType() == typeof(DatabaseServerTests)) VerifyDeletion();
        }

        void VerifyDeletion()
        {
            Debug.WriteLine("Verifying database server deletion...");
            Assert.IsNull(Subscription.DatabaseServers.AsArray().SingleOrDefault(d => d.Name == DatabaseServer.Name));            
        }
    }
}
