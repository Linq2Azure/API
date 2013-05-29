using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linq2Azure;
using System.Reactive.Linq;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IntegrationTests
{
    [TestClass]
    public class DeploymentTests : CloudServiceTests
    {
        public readonly Deployment Production, Staging;
        Deployment _retrievedProduction, _retrievedStaging;

        public DeploymentTests()
        {
            Debug.WriteLine("DeploymentTests ctor - creating production test deployment");
            Production = new Deployment("Test-Deployment1", DeploymentSlot.Production, new ServiceConfiguration(TestConstants.TestServiceConfig));
            Production.CreateAsync(CloudService, TestConstants.TestDeploymentPackageUri).Wait();

            Debug.WriteLine("DeploymentTests ctor - creating staging test deployment");
            Staging = new Deployment("Test-Deployment2", DeploymentSlot.Staging, new ServiceConfiguration(TestConstants.TestServiceConfig));
            Staging.CreateAsync(CloudService, TestConstants.TestDeploymentPackageUri).Wait();
        }

        [TestMethod]
        public async Task TestAll()
        {
            await TestCreate();
            await TestSwapDeployments();
        }

        public async Task TestCreate()
        {
            Debug.WriteLine("TestCreate: Retrieving Deployments");
            await RetrieveDeployments();
            
            Assert.IsNotNull(_retrievedProduction);
            Assert.AreEqual(_retrievedProduction.Label, Production.Label);
            Assert.AreEqual(_retrievedProduction.Slot, DeploymentSlot.Production);            

            Assert.IsNotNull(_retrievedStaging);
            Assert.AreEqual(_retrievedStaging.Label, Staging.Label);
            Assert.AreEqual(_retrievedStaging.Slot, DeploymentSlot.Staging);

            Debug.WriteLine("TestCreate: Production Refresh");
            await Production.Refresh();
            Assert.AreEqual(_retrievedProduction.Url, Production.Url);

            Debug.WriteLine("TestCreate: Staging Refresh");
            await Staging.Refresh();
            Assert.AreEqual(_retrievedStaging.Url, Staging.Url);
        }

        public async Task TestSwapDeployments()
        {
            Debug.WriteLine("TestSwapDeployments: Swapping Deployments(1)");
            await CloudService.SwapDeploymentsAsync();
            await RetrieveDeployments();

            Debug.WriteLine("TestSwapDeployments: Retrieving Deployments(1)");
            Assert.AreEqual(Production.Name, _retrievedStaging.Name);
            Assert.AreEqual(Staging.Name, _retrievedProduction.Name);

            Debug.WriteLine("TestSwapDeployments: Swapping Deployments(2)");
            await CloudService.SwapDeploymentsAsync();

            Debug.WriteLine("TestSwapDeployments: Retrieving Deployments(2)");
            await RetrieveDeployments();
            Assert.AreEqual(Production.Name, _retrievedProduction.Name);
            Assert.AreEqual(Staging.Name, _retrievedStaging.Name);
        }

        async Task RetrieveDeployments()
        {
            var deployments = await CloudService.GetDeploymentsAsync();
            _retrievedProduction = deployments.Single(d => d.Slot == DeploymentSlot.Production);
            _retrievedStaging = deployments.Single(d => d.Slot == DeploymentSlot.Staging);
        }

        public override void Dispose()
        {
            if (IsDisposed) return;
            Debug.WriteLine("Deleting test deployments");
            Production.DeleteAsync().Wait();
            Staging.DeleteAsync().Wait();
            Debug.WriteLine("Deleted test deployments");
            base.Dispose();
        }
    }
}
