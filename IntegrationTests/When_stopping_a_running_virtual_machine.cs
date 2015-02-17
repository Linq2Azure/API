using System.Linq;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_stopping_a_running_virtual_machine : VirtualMachineSetup
    {

        [TestMethod]
        public void It_should_stop_the_virtual_machine()
        {
            CreateNewVm().Wait();
            
            var deployment = CloudService.Deployments.AsArray().Single(x => x.Name == _windowsmachine);
            var role = deployment.RoleList.Single();
            role.ShutdownVirtualMachine(PostShutdownAction.Stopped).Wait();

        }

    }
}