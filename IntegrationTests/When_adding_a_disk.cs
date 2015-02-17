using Linq2Azure;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_adding_a_disk
    {

        public Subscription Subscription = null;

        [TestInitialize]
        public void TestInitialize()
        {
            Subscription = TestConstants.Subscription;
        }

        [TestMethod]
        public void It_should_provision_the_disk()
        {
            var disk = new Disk(OsType.Windows, "Windows","https://portalvhdsgmzrhtl0l1c21.blob.core.windows.net/vhds/cashconverters-cashconverters-2015-02-11.vhd");
            Subscription.CreateDiskAsync("AwesomeDisk",disk).Wait();
            disk.DeleteDiskAsync(false).Wait();
        }

    }
}