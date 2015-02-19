using System.Linq;
using System.Net;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_deleting_dns_servers_from_an_existing_deployment : VirtualMachineSetup
    {
        [TestMethod]
        public void It_should_remove_the_dns_server()
        {
            CreateNewVm().Wait();

            var deployment = CloudService.Deployments.AsArray().Single(x => x.Name == _windowsmachine);

            var dnsServer = new DnsServer("test", IPAddress.Parse("127.0.0.1"));
            deployment.AddDnsServerAsync(dnsServer).Wait();

            CloudService.Deployments.AsArray().Single(x => x.Name == _windowsmachine).DeleteDnsServerAsync(dnsServer).Wait();
            var refreshed = CloudService.Deployments.AsArray().Single(x => x.Name == _windowsmachine);


            Assert.AreEqual(0,refreshed.Dns.DnsServers.Count);
        }

    }
}