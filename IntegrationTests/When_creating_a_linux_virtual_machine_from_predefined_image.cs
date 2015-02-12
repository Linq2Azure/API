using System;
using System.Threading.Tasks;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_creating_a_linux_virtual_machine_from_predefined_image : VirtualMachineSetup
    {



        [TestMethod]
        public void It_should_create_a_new_virtual_machine()
        {
            CreateVirtualMachine().Wait();
        }

        public async Task CreateVirtualMachine()
        {

            await CloudService
                .CreateVirtualMachineDeployment("UbuntuDeployment")
                .AddRole("Ubuntu")
                .WithOSHardDisk(OperationSystemDiskLabel.Is("Ubuntu"))
                .WithDiskName("Ubuntu")
                .WithOSMedia(Os.Named("b39f27a8b8c64d52b05eac6a62ebad85__Ubuntu-14_10-amd64-server-20150202-en-us-30GB"),
                             OsDriveBlobStoredAt.LocatedAt(new Uri("https://linq2azuredev.blob.core.windows.net/vms/UbunutuLinux22.vhd")))
                .AddLinuxConfiguration(Hostname.Is("ubuntumachine"),Administrator.Is("cashconverters"), Password.Is("CashConverters1!"))
                .WithAdditionalLinuxSettings(x => x.EnableSSH = true)
                .AddNetworkConfiguration()
                .AddSSH()
                .AddWebPort()
                .Provision();
        }

    }
}