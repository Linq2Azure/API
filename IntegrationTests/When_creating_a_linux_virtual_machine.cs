using System;
using System.Threading.Tasks;
using Linq2Azure.VirtualMachines;
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
                .WithOSHardDisk(OperationSystemDiskLabel.Is("OpenSuse"))
                .WithDiskName("Main22")
                .WithOSMedia(Os.Named("openSUSE-12-3-for-Windows-Azure"), 
                             OsDriveBlobStoredAt.LocatedAt(new Uri("https://linq2azuredev.blob.core.windows.net/vms/servera2221.vhd")))
                .AddLinuxConfiguration(Hostname.Is("OpenSuseCC"), Administrator.Is("admin"), Password.Is("CashConverters1!"))
                .AddNetworkConfiguration()
                .AddSSH()
                .AddWebPort()
                .AddDisk(DiskLabel.Is("ExtraDisk"))
                .IsNew()
                .StoredAt(DriveStoredAt.LocatedAt(new Uri("https://linq2azuredev.blob.core.windows.net/vms/sd1backupa221.vhd")))
                .WithAdditionalDiskSettings(x => x.SizeInGB = 3)
                .Provision();
        }

    }
}