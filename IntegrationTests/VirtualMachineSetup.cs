using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Linq2Azure;
using Linq2Azure.CloudServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    public class VirtualMachineSetup
    {
        public const string TestLocation = "West US";
        public Subscription Subscription = TestConstants.Subscription;
        public CloudService CloudService;
        private string serviceName = "VM-" + Guid.NewGuid();

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