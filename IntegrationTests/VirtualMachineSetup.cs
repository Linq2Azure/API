using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Linq2Azure;
using Linq2Azure.CloudServices;
using Linq2Azure.VirtualMachines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    public class VirtualMachineSetup
    {
        protected const string TestLocation = "West US";
        protected Subscription Subscription = TestConstants.Subscription;
        protected CloudService CloudService;
        protected string serviceName = "VM-" + Guid.NewGuid();
        protected readonly string _windowsmachine = "Win" + Guid.NewGuid().ToString().Replace("-", String.Empty).Substring(0, 10);

        [TestInitialize]
        public void Setup()
        {
            Initialize().Wait();
        }

        protected virtual string Location()
        {
            return TestLocation;
        }

        private async Task Initialize()
        {
            CloudService = new CloudService(ServiceName(), Location(), LocationType.Region)
            {
                Description = "Virtual Machine Setup Description"
            };

            await TestConstants.Subscription.CreateCloudServiceAsync(CloudService);
            var cert = new X509Certificate2(@"..\..\CertKey.pfx", "1234", X509KeyStorageFlags.Exportable);
            var serviceCertificate = new ServiceCertificate(cert);
            await CloudService.AddServiceCertificateAsync(serviceCertificate);
        }

        protected async Task CreateNewVm()
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

        protected string ServiceName()
        {
            return serviceName;
        }

        [TestCleanup]
        public void Teardown()
        {
            CleanUp().Wait();
        }

        public async Task CleanUp()
        {
            var deployments = await CloudService.Deployments.AsTask();

            foreach (var deployment in deployments)
            {
                await deployment.DeleteAsync();
            }

            await CloudService.DeleteAsync();
        }
    }
}