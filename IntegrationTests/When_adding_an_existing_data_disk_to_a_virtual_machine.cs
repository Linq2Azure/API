using System.Linq;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{

    [TestClass]
    public class When_adding_an_existing_data_disk_to_a_virtual_machine : VirtualMachineSetup
    {

        private const string DiskName = "WindowsMachine";

        [TestMethod]
        public void it_should_add_the_disk_to_the_virtual_machine()
        {
            CreateNewVm().Wait(); var machines = from cs in Subscription.CloudServices.AsArray()
                                                 from dep in cs.Deployments.AsArray()
                                                 where dep.IsVirtualMachineDeployment.Value
                                                 from role in dep.RoleList
                                                 select role;

            var machine = machines.Single(x => x.RoleName == _windowsmachine);
            var name = _windowsmachine + "Disk";

            var disk = new Disk(OsType.Windows, DiskName, string.Format("https://linq2azuredev.blob.core.windows.net/vms/{0}.vhd", DiskName));
            Subscription.CreateDiskAsync(DiskName, disk).Wait();

            machine.AddExistingDataDiskAsync(new DataVirtualHardDisk(HostCaching.None, name, DiskName, 5, 
                                                                    "https://linq2azuredev.blob.core.windows.net/vms/" + name + "dd.vhd",
                                                                    "https://linq2azuredev.blob.core.windows.net/vms/" + DiskName + ".vhd")).Wait();

            disk.DeleteDiskAsync(false).Wait();
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