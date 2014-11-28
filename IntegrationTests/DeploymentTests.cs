using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Linq2Azure.CloudServices;

namespace IntegrationTests
{
    [TestClass]
    public class DeploymentTests
    {
        public readonly CloudServiceTests Parent = new CloudServiceTests();
        public readonly Deployment Production, Staging;
        Deployment _retrievedProduction, _retrievedStaging;
        public CloudService CloudService { get { return Parent.CloudService; } }

        public DeploymentTests()
        {
            Debug.WriteLine("DeploymentTests ctor - creating production test deployment");
            Production = new Deployment("Test-Deployment1", DeploymentSlot.Production, new ServiceConfiguration(TestConstants.TestServiceConfig));
            CloudService.PublishDeploymentAsync(Production, TestConstants.TestDeploymentPackageUri).Wait();

            Debug.WriteLine("DeploymentTests ctor - creating staging test deployment");
            Staging = new Deployment("Test-Deployment2", DeploymentSlot.Staging, TestConstants.TestServiceConfigString);
            CloudService.PublishDeploymentAsync(Staging, TestConstants.TestDeploymentPackageUri).Wait();
        }
        
        [TestMethod]
        public async Task CanUseDeployment()
        {
            await CanCreate();
            await CanSwapDeployments();
            await CanStartStop();
            await CanUpdateConfiguraion();
            await CanUpdateDeployment();
            await CanDelete();
        }

        private async Task CanDelete()
        {
            await _retrievedProduction.DeleteAsync();
            await _retrievedStaging.DeleteAsync();
        }

        private async Task CanUpdateDeployment()
        {
            await RetrieveDeployments();
            var label = string.Format("{0}2", _retrievedProduction.Label);
            _retrievedProduction.Label = label;
            await _retrievedProduction.UpgradeAsync(
                TestConstants.TestDeploymentPackageUri,
                deploymentType: DeploymentType.Forced);

            await RetrieveDeployments();

            Assert.IsNotNull(_retrievedProduction);
            Assert.AreEqual(_retrievedProduction.Label, label);
        }

        private async Task CanUpdateConfiguraion()
        {
            await RetrieveDeployments();

            var roleConfig = _retrievedProduction.Configuration.Roles.Single();
            roleConfig.InstanceCount = 2;
            await _retrievedProduction.UpdateConfigurationAsync();
        }

        public async Task CanCreate()
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
            await Production.RefreshAsync();
            Assert.AreEqual(_retrievedProduction.Url, Production.Url);

            Debug.WriteLine("TestCreate: Staging Refresh");
            await Staging.RefreshAsync();
            Assert.AreEqual(_retrievedStaging.Url, Staging.Url);
        }

        public async Task CanStartStop()
        {
            Debug.WriteLine("TestStartStop: Starting Deployment");
            await Production.StartAsync();
            string powerState = (await Production.RoleInstances.AsTask()).First().PowerState;
            Assert.IsTrue(powerState == "Started" || powerState == "Starting");
            await Production.StopAsync();
            powerState = (await Production.RoleInstances.AsTask()).First().PowerState;
            Assert.IsTrue(powerState == "Stopped" || powerState == "Stopping");
        }

        public async Task CanSwapDeployments()
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
            var deployments = await CloudService.Deployments.AsTask();
            _retrievedProduction = deployments.Single(d => d.Slot == DeploymentSlot.Production);
            _retrievedStaging = deployments.Single(d => d.Slot == DeploymentSlot.Staging);
        }

        public virtual void Dispose()
        {
            bool verifyDeletion = Production.Parent != null || Staging.Parent != null;

            if (Production.Parent != null)
            {
                Debug.WriteLine("Deleting test production deployment");
                Production.DeleteAsync().Wait();
            }
            if (Staging.Parent != null)
            {
                Debug.WriteLine("Deleting test staging deployment");
                Staging.DeleteAsync().Wait();
            }

            if (verifyDeletion && GetType() == typeof(DeploymentTests)) VerifyDeletion();

            Parent.Dispose();
        }

        void VerifyDeletion()
        {
            Debug.WriteLine("Verifying deployment deletion");
            Assert.AreEqual(0, CloudService.Deployments.AsArray().Length);
        }
    }
}
