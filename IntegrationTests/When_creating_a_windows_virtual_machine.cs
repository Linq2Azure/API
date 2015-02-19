using System;
using System.Threading.Tasks;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_creating_a_windows_virtual_machine : VirtualMachineSetup
    {

        [TestMethod]
        public void It_should_create_a_new_virtual_machine()
        {
            CreateVirtualMachine().Wait();
        }

        protected override string Location()
        {
            return "Australia East";
        }

        public async Task CreateVirtualMachine()
        {

            await CloudService
                .CreateVirtualMachineDeployment("WindowsFromGeneralized")
                .AddRole("WindowsFromGeneralized")
                .WithOSHardDisk(OperationSystemDiskLabel.Is("Windows1"))
                .WithAutoGeneratedDiskName()
                .WithImageMedia(ImageName.Named("CC2Win008R2-os-2015-02-11"), 
                                OsDriveBlobStoredAt.LocatedAt(new Uri("https://linq2azuredev.blob.core.windows.net/vms/Windows33.vhd")))
                .AddWindowsConfiguration(ComputerName.Is("Windows1"), Administrator.Is("CashConverters"), Password.Is("CashConverters1"))
                .AddNetworkConfiguration()
                .AddRemoteDesktop()
                .AddWebPort()
                .Provision();
        }

    }

    
}