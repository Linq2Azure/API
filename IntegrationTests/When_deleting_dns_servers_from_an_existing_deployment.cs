using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_deleting_dns_servers_from_an_existing_deployment : VirtualMachineSetup
    {

        private readonly string _windowsmachine = "Win" + Guid.NewGuid().ToString().Replace("-", String.Empty).Substring(0, 10);

        [TestMethod]
        public void It_should_remove_the_dns_server()
        {
            CreateVirtualMachine().Wait();

            var deployment = CloudService.Deployments.AsArray().Single(x => x.Name == _windowsmachine);

            var dnsServer = new DnsServer("test", IPAddress.Parse("127.0.0.1"));
            deployment.AddDnsServerAsync(dnsServer).Wait();

            CloudService.Deployments.AsArray().Single(x => x.Name == _windowsmachine).DeleteDnsServerAsync(dnsServer).Wait();
            var refreshed = CloudService.Deployments.AsArray().Single(x => x.Name == _windowsmachine);


            Assert.AreEqual(0,refreshed.Dns.DnsServers.Count);
        }

        public async Task CreateVirtualMachine()
        {

            await CloudService
                .CreateVirtualMachineDeployment(_windowsmachine)
                .AddRole(_windowsmachine)
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