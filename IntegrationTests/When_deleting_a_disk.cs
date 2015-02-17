using System;
using System.Linq;
using Linq2Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_deleting_a_disk
    {
        public Subscription Subscription = null;

        [TestInitialize]
        public void TestInitialize()
        {
            Subscription = TestConstants.Subscription;
        }

        [TestMethod]
        public void it_should_have_disks()
        {
            var results = Subscription.VirtualMachineDisks.AsArray();

            foreach (var result in results)
                Console.WriteLine(result);

            const string mediaLink = "https://linq2azuredev.blob.core.windows.net/vms/Wine768f9a5a2.vhd";

            results.Single(x => x.MediaLink.Equals(mediaLink))
                .DeleteDiskAsync(false).Wait();

            Assert.IsFalse(Subscription.VirtualMachineDisks
                                       .AsArray()
                                       .Any(x => x.MediaLink.Equals(mediaLink)));
        }
    }
}