using System.Threading.Tasks;
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
                .AddRole("OpenSuse")
                .WithSize("Small")
                .WithOsVerion()
                .WithOSHardDisk("Ubuntu")
                .WithMediaLink("https://linq2azuredev.blob.core.windows.net/vms/UbunutuLinux1.vhd")
                .WithSourceImageName("b39f27a8b8c64d52b05eac6a62ebad85__Ubuntu-14_10-amd64-server-20150202-en-us-30GB")
                .Continue()
                .AddLinuxConfiguration()
                .WithComputerName("CCUbuntu01")
                .WithSSHEnabled()
                .WithUserName("admin")
                .WithUserPassword("Blair@015!")
                .WithHostname("CCUbuntu01")
                .AddNetworkConfiguration()
                .AddSSH()
                .AddWebPort()
                .FinalizeRoles()
                .Provision();
        }

    }
}