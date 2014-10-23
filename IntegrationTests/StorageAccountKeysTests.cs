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
    public class StorageAccountKeysTests : IDisposable
    {
        public readonly Subscription Subscription = TestConstants.Subscription;

        public readonly StorageAccount StorageAccount = new StorageAccount(
            "l2aintegrationtest" + new Random().Next(0, 100000),
            "Integration Test",
            "West US",
            LocationType.Region,
            StorageAccountType.Standard_RAGRS);

        public StorageAccountKeysTests()
        {
            Debug.WriteLine("Creating storage account...");
            Subscription.CreateStorageAccountAsync(StorageAccount).Wait();
        }

        [TestMethod]
        public async Task CanUpdateStorageAccountKeys()
        {
            var secondaryKey = StorageAccount.Keys
                .AsArray()
                .Single(k => k.KeyType == KeyType.Secondary);

            var originalKeyValue = secondaryKey.Key;

            await secondaryKey.RegenerateKeyAsync();

            var updatedKeyValue = StorageAccount.Keys
                .AsArray()
                .Single(k => k.KeyType == KeyType.Secondary)
                .Key;

            Assert.AreNotEqual(originalKeyValue, updatedKeyValue);
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