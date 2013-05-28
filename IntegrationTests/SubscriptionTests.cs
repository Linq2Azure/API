using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linq2Azure;
using System.Reactive.Linq;

namespace IntegrationTests
{
    [TestClass]
    public class SubscriptionTests
    {
        [TestMethod]
        public void CanLoadFromPublisherSettingsFile()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(TestConstants.Subscription.SubscriptionName));
            Assert.IsTrue(TestConstants.Subscription.SubscriptionID != Guid.Empty);
            Assert.IsNotNull(TestConstants.Subscription.ManagementCertificate);
        }

        [TestMethod]
        public void CanListCloudServices()
        {
            Assert.IsTrue(TestConstants.Subscription.CloudServices.Count().Wait() > 0);
        }
    }
}
