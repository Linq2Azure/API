using System.Linq;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_updating_an_existing_disks_host_caching : VirtualMachineSetup
    {

        [TestMethod]
        public void it_should_update_the_disks_host_caching()
        {

            CreateNewVm().Wait();

            var machines = from cs in Subscription.CloudServices.AsArray()
                from dep in cs.Deployments.AsArray()
                where dep.IsVirtualMachineDeployment.Value
                from role in dep.RoleList
                select role;

            var machine = machines.Single(x => x.RoleName == _windowsmachine);
            var name = _windowsmachine + "Disk";

            AddDisk(name, machine);
            UpdateDisk(name);

            var disk = FindDisk(name);
            disk.DeleteDiskAsync(false).Wait();

            Assert.AreEqual(HostCaching.ReadWrite, disk.HostCaching);

        }

        private void AddDisk(string name, Role machine)
        {
            var disk = new DataVirtualHardDisk(HostCaching.None, name, name, 5, 5,
                "https://linq2azuredev.blob.core.windows.net/vms/" + _windowsmachine + "disk.vhd");
            machine.AddEmptyDataDiskAsync(disk).Wait();
        }

        public void UpdateDisk(string name)
        {
            var disk = FindDisk(name);
            disk.ChangeOrDefaultHostCachingAsync(HostCaching.ReadWrite).Wait();
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