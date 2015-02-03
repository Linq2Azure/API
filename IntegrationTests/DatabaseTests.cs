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

        public DatabaseTests()
        {
            Debug.WriteLine("Creating database server...");
            Subscription.CreateDatabaseServerAsync(DatabaseServer, Password).Wait();
        }

        [TestMethod]
        public async Task CanGetDatabases()
        {
            var database = (await Subscription.DatabaseServers.AsTask()).Single(d => d.Name == DatabaseServer.Name);

            Assert.AreNotEqual(0, database.Databases.AsArray().Length);
        }

        public void Dispose()
        {
            if (DatabaseServer.Subscription == null)
            {
                return;
            }
            Debug.WriteLine("Dropping server...");
            DatabaseServer.DropAsync().Wait();

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