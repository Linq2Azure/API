using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linq2Azure;
using System.Reactive.Linq;

namespace IntegrationTests
{
    [TestClass]
    public class CloudServiceTests
    {
        const string TestLocation = "West US";

        [TestMethod]
        public void CanCRUDCloudService()
        {
            Subscription sub = TestConstants.Subscription;
            var cs = new CloudService
            {
                Location = TestLocation,
                ServiceName = "Test-" + Guid.NewGuid().ToString("N")
            };
            cs.Label = cs.ServiceName;
            cs.CreateAsync(sub).Wait();

            var cs2 = sub.CloudServices.SingleOrDefaultAsync(s => s.Label == cs.Label && s.ServiceName == cs.ServiceName).Wait();
            Assert.IsNotNull(cs2, "Creation failed");

            cs2.DeleteAsync().Wait();

            var cs3 = sub.CloudServices.FirstOrDefaultAsync(s => s.Label == cs.Label && s.ServiceName == cs.ServiceName).Wait();
            Assert.IsNull(cs3, "Deletion failed");
        }
    }
}
