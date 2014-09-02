using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Linq2Azure;
using Linq2Azure.AffinityGroups;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class AffinityGroupTests : IDisposable
    {
        public readonly Subscription Subscription = TestConstants.Subscription;
        public readonly AffinityGroup AffinityGroup = new AffinityGroup("linq2azuretest", "linq2azuretest", "The Linq2Azure Affinity Group", "West US");

        public AffinityGroupTests()
        {
            Debug.WriteLine("Creating affinity group...");
            Subscription.CreateAffinityGroupAsync(AffinityGroup).Wait();
        }

        [TestMethod]
        public async Task CanUseAffinityGroup()
        {
            await CanCreateAffinityGroup();
            await CanUpdateDetails();
        }

        private async Task CanCreateAffinityGroup()
        {
            Assert.IsNotNull(AffinityGroup.Name);
            Assert.IsNotNull(AffinityGroup.Subscription);
            Debug.WriteLine("Retrieving affinity group...");
            Assert.IsNotNull((await Subscription.AffinityGroups.AsTask()).SingleOrDefault(d => d.Name == AffinityGroup.Name));
        }

        private async Task CanUpdateDetails()
        {
            AffinityGroup.Label = "newlabel";
            AffinityGroup.Description = "New Description";

            await AffinityGroup.UpdateAsync();

            var updatedGroup = Subscription.AffinityGroups.AsArray().First(a => a.Name == "linq2azuretest");

            Assert.AreEqual("newlabel", updatedGroup.Label);
            Assert.AreEqual("New Description", updatedGroup.Description);
        }

        public void Dispose()
        {
            if (AffinityGroup.Subscription == null)
                return;

            AffinityGroup.DeleteAsync().Wait();

            VerifyDeletion();
        }

        void VerifyDeletion()
        {
            Debug.WriteLine("Verifying affinity group deletion...");
            Assert.IsNull(Subscription.AffinityGroups.AsArray().SingleOrDefault(d => d.Name == AffinityGroup.Name));            
        }
    }
}