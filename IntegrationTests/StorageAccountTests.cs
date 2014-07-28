using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Linq2Azure;
using Linq2Azure.CloudServices;
using Linq2Azure.StorageAccounts;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class StorageAccountTests : IDisposable
    {
        public readonly Subscription Subscription = TestConstants.Subscription;

        public readonly StorageAccount StorageAccount = new StorageAccount(
            "l2aintegrationtest" + new Random().Next(0, 100000),
            "Integration Test",
            "West US",
            DeploymentAssociation.Location,
            StorageAccountGeoReplication.ReadAccessEnabled);

        public StorageAccountTests()
        {
            Debug.WriteLine("Creating storage account...");
            Subscription.CreateStorageAccountAsync(StorageAccount).Wait();
        }

        [TestMethod]
        public async Task CanUseDatabaseServer()
        {
            await CanCreateDatabaseServer();
        }

        async Task CanCreateDatabaseServer()
        {
            Assert.IsNotNull(StorageAccount.ServiceName);
            Assert.IsNotNull(StorageAccount.Subscription);
            Debug.WriteLine("Retrieving storage account...");
            Assert.IsNotNull((await Subscription.StorageAccounts.AsTask()).SingleOrDefault(d => d.ServiceName == StorageAccount.ServiceName));
        }

        public void Dispose()
        {
            if (StorageAccount.Subscription == null)
            {
                return;
            }
            Debug.WriteLine("Deleting storage account...");
            StorageAccount.DeleteAsync().Wait();

            // Verify the deletion only if we're testing the database server itself.
            if (GetType() == typeof(DatabaseServerTests))
            {
                VerifyDeletion();
            }
        }

        private void VerifyDeletion()
        {
            Debug.WriteLine("Verifying storage account deletion...");
            Assert.IsNull(Subscription.StorageAccounts.AsArray().SingleOrDefault(d => d.ServiceName == StorageAccount.ServiceName));
        }
    }
}