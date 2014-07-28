﻿using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class SubscriptionTests
    {
        [TestMethod]
        public void CanLoadFromPublisherSettingsFile()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(TestConstants.Subscription.Name));
            Assert.IsTrue(TestConstants.Subscription.ID != Guid.Empty);
            Assert.IsNotNull(TestConstants.Subscription.ManagementCertificate);
        }

        [TestMethod]
        public async Task CleanupOldResidue()
        {
            DateTime cutoff = DateTime.UtcNow.AddMinutes(-5);

            foreach (var cs in await TestConstants.Subscription.CloudServices.AsTask())
                if (cs.Name.StartsWith("Test-") && cs.DateLastModified < cutoff)
                {
                    foreach (var d in await cs.Deployments.AsTask()) await d.DeleteAsync();
                    await cs.DeleteAsync();
                }

            foreach (var s in await TestConstants.Subscription.DatabaseServers.AsTask()) 
                if (s.AdministratorLogin == "testadmin")
                    await s.DropAsync();

            foreach (var s in await TestConstants.Subscription.TrafficManagerProfiles.AsTask())
                if (s.Name.StartsWith(("Test-")))
                    await s.DeleteAsync();
        }
    }
}
