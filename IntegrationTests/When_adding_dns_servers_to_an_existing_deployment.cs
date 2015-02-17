using System.Linq;
using System.Net;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_adding_dns_servers_to_an_existing_deployment : VirtualMachineSetup
    {

        [TestMethod]
        public void It_should_add_the_dns_server()
        {
            CreateNewVm().Wait();

            var deployment = CloudService.Deployments.AsArray().Single(x => x.Name == _windowsmachine);

            deployment.AddDnsServerAsync(new DnsServer("test", IPAddress.Parse("127.0.0.1"))).Wait();

            var refreshed = CloudService.Deployments.AsArray().Single(x => x.Name == _windowsmachine);
            var contains = refreshed.Dns.DnsServers.Any(x => x.Name == "test");
            Assert.IsTrue(contains);
        }

    }

   
}