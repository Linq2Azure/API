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
        public void it_should_list_the_disk_in_the_user_repository()
        {
            const string name = "WindowsMachine";

            var disk = new Disk(OsType.Windows, name, string.Format("https://linq2azuredev.blob.core.windows.net/vms/{0}.vhd", name));
            Subscription.CreateDiskAsync(name, disk).Wait();
            disk.DeleteDiskAsync(false).Wait();
        }

    }
}