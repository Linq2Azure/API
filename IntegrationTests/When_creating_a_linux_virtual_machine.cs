using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_creating_a_linux_virtual_machine : VirtualMachineSetup
    {

        protected override string Location()
        {
            return "West US";
        }

        [TestMethod]
        public void It_should_create_a_new_virtual_machine()
        {
            CreateVirtualMachine().Wait();
        }

        public async Task CreateVirtualMachine()
        {

            await CloudService
                .CreateVirtualMachineDeployment("SuseDeployment2")
                .AddRole("OpenSuse2")
                .WithSize("Small")
                .WithOSHardDisk("OpenSuse")
                .WithDiskName("Main22")
                .WithMediaLink("https://linq2azuredev.blob.core.windows.net/vms/servera2221.vhd")
                .WithSourceImageName("openSUSE-12-3-for-Windows-Azure")
                .Continue()
                .WithDataDiskConfiguration("ExtraDisk")
                .AddDisk()
                .WithDataDiskLabel("Backup")
                .WithDataDiskLogicalSizeInGB(3)
                .WithDataDiskLun(0)
                .WithDataDiskMediaLink("https://linq2azuredev.blob.core.windows.net/vms/sd1backupa221.vhd")
                .FinishedAddingDataDisk()
                .FinsishedDataDiskConfiguration()
                .AddLinuxConfiguration()
                .WithComputerName("AwesomePC2")
                .WithSSHEnabled()
                .WithUserName("admin")
                .WithUserPassword("Blair@015!")
                .WithHostname("ccopensuse2")
                .AddNetworkConfiguration()
                .AddSSH()
                .AddWebPort()
                .FinalizeRoles()
                .Provision();
        }

    }
}