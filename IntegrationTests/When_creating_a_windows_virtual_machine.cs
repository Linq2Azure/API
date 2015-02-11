using System.Threading.Tasks;
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

        public async Task CreateVirtualMachine()
        {

            await CloudService
                .CreateVirtualMachineDeployment("Windows3")
                .AddRole("Windows3")
                .WithSize("Small")
                .WithOsVerion()
                .WithOSHardDisk("Main3")
                .WithSourceImageName("CC2Win008R2-os-2015-02-11")
                .WithMediaLink("https://linq2azuredev.blob.core.windows.net/vms/wserver221.vhd")
                .Continue()
                .AddWindowsConfiguration()
                .ComputerName("Windows3")
                .AdminUsername("CashConverters")
                .AdminPassword("CashConverters1")
                .AddNetworkConfiguration()
                .AddRemoteDesktop()
                .AddWebPort()
                .FinalizeRoles()
                .Provision();
        }

    }
}