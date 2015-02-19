using System.Linq;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_adding_an_empty_data_disk_to_a_virtual_machine : VirtualMachineSetup
    {

        [TestMethod]
        public void it_should_add_the_disk_to_the_vm()
        {

            CreateNewVm().Wait();

            var machines = from cs in Subscription.CloudServices.AsArray()
                from dep in cs.Deployments.AsArray()
                where dep.IsVirtualMachineDeployment.Value
                from role in dep.RoleList
                select role;

            var machine = machines.Single(x => x.RoleName == _windowsmachine);
            var name = _windowsmachine + "Disk";

            machine.AddEmptyDataDiskAsync(new DataVirtualHardDisk(HostCaching.None, name, name, 5,5, "https://linq2azuredev.blob.core.windows.net/vms/" + _windowsmachine + "disk.vhd")).Wait();

            Assert.IsNotNull(HasTheDiskAdded(name));
        }

        public DataVirtualHardDisk HasTheDiskAdded(string name)
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