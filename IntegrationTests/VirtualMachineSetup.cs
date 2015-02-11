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
        public const string TestLocation = "Australia East";
        public Subscription Subscription = TestConstants.Subscription;
        public CloudService CloudService;

        [TestInitialize]
        public void Setup()
        {
            Initialize().Wait();
        }

        private async Task Initialize()
        {
            CloudService = new CloudService("VM-" + Guid.NewGuid().ToString("N"), TestLocation, LocationType.Region)
            {
                Description = "Virtual Machine Setup Description"
            };

            await TestConstants.Subscription.CreateCloudServiceAsync(CloudService);
            var cert = new X509Certificate2(@"..\..\CertKey.pfx", "1234", X509KeyStorageFlags.Exportable);
            var serviceCertificate = new ServiceCertificate(cert);
            await CloudService.AddServiceCertificateAsync(serviceCertificate);
        }

        [TestCleanup]
        public void Teardown()
        {
            CleanUp().Wait();
        }

        public async Task CleanUp()
        {
            // await CloudService.DeleteDeploymentSlot("Production");
            //await CloudService.DeleteAsync();
        }
    }
}