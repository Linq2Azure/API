using System;
using System.Threading.Tasks;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_adding_extensions_to_a_vm: VirtualMachineSetup
    {

        [TestMethod]
        public void It_should_create_the_vm_with_the_extensions()
        {
            CreateVm().Wait();
        }

        protected async Task CreateVm()
        {

            await CloudService
                .CreateVirtualMachineDeployment(_windowsmachine)
                .AddRole(_windowsmachine,RoleSize.Small,
                    x =>
                    {
                        x.ProvisionGuestAgent = true;
                        x.AddAntiMalware().AddDocker().AddPowershell().AddSystemCenter().AddVS14CTPDebugger().Build();
                    })
                .WithOSHardDisk(OperationSystemDiskLabel.Is(_windowsmachine))
                .WithDiskName(_windowsmachine)
                .WithOSMedia(Os.Named("03f55de797f546a1b29d1b8d66be687a__Visual-Studio-2013-Community-12.0.31101.0-AzureSDK-2.5-WS2012R2"),
                    OsDriveBlobStoredAt.LocatedAt(new Uri("https://linq2azuredev.blob.core.windows.net/vms/" + _windowsmachine + ".vhd")))
                .AddWindowsConfiguration(ComputerName.Is(_windowsmachine), Administrator.Is("CashConverters"), Password.Is("CashConverters1"))
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