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

    [TestClass]
    public class When_creating_a_linux_virtual_machine : VirtualMachineSetup
    {

        

        [TestMethod]
        public void It_should_create_a_new_virtual_machine()
        {
            CreateVirtualMachine().Wait();
        }

        public async Task CreateVirtualMachine()
        {

            await CloudService
                .CreateVirtualMachineDeployment("SuseDeployment")
                .AddRole("OpenSuse")
                .WithSize("Small")
                .WithOSHardDisk("OpenSuse")
                .WithDiskName("Main")
                .WithMediaLink("https://linq2azuredev.blob.core.windows.net/vms/server.vhd")
                .WithSourceImageName("openSUSE-12-3-for-Windows-Azure")
                .Continue()
                .WithDataDiskConfiguration("ExtraDisk")
                .AddDisk()
                .WithDataDiskLabel("Backup")
                .WithDataDiskLogicalSizeInGB(3)
                .WithDataDiskLun(0)
                .WithDataDiskMediaLink("https://linq2azuredev.blob.core.windows.net/vms/sd1backup.vhd")
                .FinishedAddingDataDisk()
                .FinsishedDataDiskConfiguration()
                .AddLinuxConfiguration()
                .WithComputerName("AwesomePC")
                .WithUserName("admin")
                .WithUserPassword("Blair@015!")
                .WithHostname("ccopensuse")
                .AddNetworkConfiguration()
                .AddRemoteDesktop()
                .AddWebPort()
                .FinalizeRoles()
                .Provision();
        }

    }

    [TestClass]
    public class When_creating_a_windows_virtual_machine : VirtualMachineSetup
    {



        [TestMethod]
        public void It_should_create_a_new_virtual_machine()
        {
            CreateVirtualMachine().Wait();
        }

        public async Task CreateVirtualMachine()
        {

            await CloudService
                .CreateVirtualMachineDeployment("WindowsDeployment")
                .AddRole("Windows")
                .WithSize("Small")
                .WithOSHardDisk("Windows")
                .WithDiskName("Main")
                .WithMediaLink("https://linq2azuredev.blob.core.windows.net/vms/server2.vhd")
                .WithSourceImageName("a699494373c04fc0bc8f2bb1389d6106__Windows-Server-2012-R2-201412.01-en.us-127GB.vhd")
                .Continue()
                .AddWindowsConfiguration()
                .ComputerName("AwesomePC22")
                .AdminPassword("Blair@015!")
                .AddNetworkConfiguration()
                .AddRemoteDesktop()
                .AddWebPort()
                .FinalizeRoles()
                .Provision();
        }

    }
}