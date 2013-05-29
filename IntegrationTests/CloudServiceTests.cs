using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linq2Azure;
using System.Reactive.Linq;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IntegrationTests
{
    [TestClass]
    public class CloudServiceTests : IDisposable
    {
        public const string TestLocation = "West US";
        public readonly Subscription Subscription = TestConstants.Subscription;
        public readonly CloudService CloudService;
        public bool IsDisposed { get; private set; }

        public CloudServiceTests()
        {
            CloudService = new CloudService("Test-" + Guid.NewGuid().ToString("N"), CloudServiceTests.TestLocation);
            CloudService.Description = "Test Description";
            Debug.WriteLine("CloudServiceTests ctor - creating test service");
            CloudService.CreateAsync(TestConstants.Subscription).Wait();
        }

        [TestMethod]
        public async Task CanCreateAndRefresh()
        {
            CloudService cs1 = CloudService;
            var cs2 = (await Subscription.GetCloudServicesAsync()).SingleOrDefault(s => s.Label == cs1.Label && s.Name == cs1.Name);
            Assert.IsNotNull(cs2, "Creation failed");

            Assert.AreEqual("Created", cs2.Status);
            Assert.AreEqual(cs1.Name, cs2.Name);
            Assert.AreEqual(TestLocation, cs2.Location);
            Assert.AreEqual(cs1.Description, cs2.Description);

            cs1.Refresh().Wait();
            Assert.AreEqual(cs1.DateCreated, cs2.DateCreated);
            Assert.AreEqual(cs1.DateLastModified, cs2.DateLastModified);
            Assert.AreEqual(cs1.Description, cs2.Description);
            Assert.AreEqual(cs1.Label, cs2.Label);
            Assert.AreEqual(cs1.Url, cs2.Url);
        }

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            Debug.WriteLine("Deleting test CloudService");
            CloudService.DeleteAsync().Wait();
            Debug.WriteLine("Deleted test CloudService");
            IsDisposed = true;
            if (GetType() == typeof(CloudServiceTests))
            {
                var cs = Subscription.GetCloudServicesAsync().Result.FirstOrDefault(s => s.Name == CloudService.Name);
                Assert.IsNull(cs, "Deletion failed");
            }
        }
    }
}
