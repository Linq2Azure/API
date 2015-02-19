using System.Linq;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_deleting_a_data_disk : VirtualMachineSetup
    {

        [TestMethod]
        public void it_should_remove_the_data_disk()
        {

            CreateNewVm().Wait();

            var machines = from cs in Subscription.CloudServices.AsArray()
                from dep in cs.Deployments.AsArray()
                where dep.IsVirtualMachineDeployment.Value
                from role in dep.RoleList
                select role;

            var machine = machines.Single(x => x.RoleName == _windowsmachine);
            var name = _windowsmachine + "Disk";

            var disk = new DataVirtualHardDisk(HostCaching.None, name, name, 5, 5, "https://linq2azuredev.blob.core.windows.net/vms/" + _windowsmachine + "disk.vhd");
            machine.AddEmptyDataDiskAsync(disk).Wait();

            var diskToRemove = FindDisk(name);
            diskToRemove.DeleteDiskAsync(false).Wait();

            Assert.IsNull(FindDisk(name));
        }

        public DataVirtualHardDisk FindDisk(string name)
        {
            var machines = from cs in Subscription.CloudServices.AsArray()
                           from dep in cs.Deployments.AsArray()
                           where dep.IsVirtualMachineDeployment.Value
                           from role in dep.RoleList
                           select role;

            var machine = machines.Single(x => x.RoleName == _windowsmachine);

            return machine.DataVirtualHardDisks.SingleOrDefault(x => x.DiskLabel == name);
        }
    }
}