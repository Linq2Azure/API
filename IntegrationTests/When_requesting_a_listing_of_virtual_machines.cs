using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_requesting_a_listing_of_virtual_machines : VirtualMachineSetup
    {

        [TestMethod]
        public void It_should_list_all_the_virtual_machines_under_all_the_accounts()
        {

            CreateNewVm().Wait();

            var machines = from cs in Subscription.CloudServices.AsArray()
                from dep in cs.Deployments.AsArray()
                where dep.IsVirtualMachineDeployment.Value
                from role in dep.RoleList
                select role;

            Assert.AreNotEqual(0,machines.Count());

        }

    }
}