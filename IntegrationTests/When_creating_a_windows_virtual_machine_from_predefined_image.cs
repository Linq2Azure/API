using System;
using System.Threading.Tasks;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_creating_a_windows_virtual_machine_from_predefined_image : VirtualMachineSetup
    {



        [TestMethod]
        public void It_should_create_a_new_virtual_machine()
        {
            CreateVirtualMachine().Wait();
        }

        public async Task CreateVirtualMachine()
        {

            await CloudService
                .CreateVirtualMachineDeployment("WinPreDeployment")
                .AddRole("Windows3")
                .WithOSHardDisk(OperationSystemDiskLabel.Is("Windows3"))
                .WithDiskName("Windows3")
                .WithOSMedia(Os.Named("03f55de797f546a1b29d1b8d66be687a__Visual-Studio-2013-Community-12.0.31101.0-AzureSDK-2.5-WS2012R2"),
                             OsDriveBlobStoredAt.LocatedAt(new Uri("https://linq2azuredev.blob.core.windows.net/vms/Windows5.vhd")))
                .AddWindowsConfiguration(ComputerName.Is("WindowsMachine"), Administrator.Is("CashConverters"), Password.Is("CashConverters1"))
                .WithAdditionalWindowsSettings(x =>
                {
                    x.EnableAutomaticUpdates = true;
                    x.ResetPasswordOnFirstLogin = true;
                })
                .AddNetworkConfiguration()
                .AddRemoteDesktop()
                .AddWebPort()
                .Provision();



        }

    }
}