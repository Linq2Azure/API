using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Xml.Linq;
using Linq2Azure.CloudServices;

namespace Linq2Azure.VirtualMachines
{
    public class Role
    {
        public static string VirtualMachineRoleType = "PersistentVMRole";

        public Role()
        {
            ConfigurationSets = new List<ConfigurationSet>();
            ResourceExtensionReferences = new List<ResourceExtensionReference>();
            DataVirtualHardDisks = new List<DataVirtualHardDisk>();
            OSVirtualHardDisk = new OSVirtualHardDisk();
            VmImageInput = new VImageInput();
            RoleSize = RoleSize.Small;
        }

        public Role(IDeployment deployment, XElement element)
            : this()
        {
            Deployment = deployment;
            element.HydrateObject(XmlNamespaces.WindowsAzure, this);
            DataVirtualHardDisks.ForEach(x => x.AssignRole(this));
        }

        public Role(string roleType, string roleName, RoleSize roleSize) : this()
        {
            Contract.Requires(!String.IsNullOrEmpty(roleType));
            Contract.Requires(!String.IsNullOrEmpty(roleName));

            RoleType = roleType;
            RoleName = roleName;
            RoleSize = roleSize;
        }

        internal void ChangeOsVirtualHardDisk(OSVirtualHardDisk disk)
        {
            Contract.Requires(disk != null);
            OSVirtualHardDisk = disk;
        }

        internal void ChangeAvailabilitySetName(string availabilitySetName)
        {
            AvailabilitySetName = availabilitySetName;
        }

        internal void ChangeProvisionGuestAgent(bool provision)
        {
            ProvisionGuestAgent = provision;
        }

        public async Task DeleteVirtualMachineAsync(bool removeAssociatedDisksAndBlobs)
        {
           
            var suffix = Deployment.Name + "/roles/" + RoleName;
            var queryString = removeAssociatedDisksAndBlobs ? "?comp=media" : String.Empty;
             
            var client = GetRestClient(suffix,queryString);
            var response = await client.DeleteAsync();
            await Deployment.GetCloudService().Subscription.WaitForOperationCompletionAsync(response);
        }

        public async Task StartVirtualMachine()
        {
            var suffix = Deployment.Name + "/roleinstances/" + RoleName + "/Operations";
            var content = new XElement(XmlNamespaces.WindowsAzure + "StartRoleOperation");
            content.Add(new XElement(XmlNamespaces.WindowsAzure + "OperationType", RoleOperationType.StartRoleOperation.ToString()));


            var client = GetRestClient(suffix);
            var response = await client.PostAsync(content);
            await Deployment.GetCloudService().Subscription.WaitForOperationCompletionAsync(response);
        }

        public async Task RestartVirtualMachine()
        {
            var suffix = Deployment.Name + "/roleinstances/" + RoleName + "/Operations";
            var content = new XElement(XmlNamespaces.WindowsAzure + "RestartRoleOperation");
            content.Add(new XElement(XmlNamespaces.WindowsAzure + "OperationType", RoleOperationType.RestartRoleOperation.ToString()));


            var client = GetRestClient(suffix);
            var response = await client.PostAsync(content);
            await Deployment.GetCloudService().Subscription.WaitForOperationCompletionAsync(response);
        }

        public async Task ShutdownVirtualMachine(PostShutdownAction postShutdownAction)
        {
            var suffix = Deployment.Name + "/roleinstances/" + RoleName + "/Operations";
            var content = new XElement(XmlNamespaces.WindowsAzure + "ShutdownRoleOperation");
            content.Add(new XElement(XmlNamespaces.WindowsAzure + "OperationType", RoleOperationType.ShutdownRoleOperation.ToString()),
                        new XElement(XmlNamespaces.WindowsAzure + "PostShutdownAction", postShutdownAction.ToString()));


            var client = GetRestClient(suffix);
            var response = await client.PostAsync(content);
            await Deployment.GetCloudService().Subscription.WaitForOperationCompletionAsync(response);
        }

        public Task AddEmptyDataDiskAsync(DataVirtualHardDisk disk)
        {
            return disk.AddEmptyDataDiskAsync(this);
        }

        public Task AddExistingDataDiskAsync(DataVirtualHardDisk disk)
        {
            return disk.AddExistingDataDiskAsync(this);
        }

        private AzureRestClient GetRestClient(string suffix = "", string queryString = "")
        {
            var cloudService = Deployment.GetCloudService();
            var servicePath = "services/hostedservices/" + cloudService.Name + "/deployments/" + suffix + queryString;
            var client = cloudService.Subscription.GetDatabaseRestClient(servicePath);
            return client;
        }

        [Ignore]
        public IDeployment Deployment { get; private set; }

        public string RoleName { get; private set; }
        public string RoleType { get; private set; }

        public bool IsVirtualMachineRole()
        {
            return RoleType == VirtualMachineRoleType;
        }

        [Traverse]
        public List<ConfigurationSet> ConfigurationSets { get; set; }
        public string VMImageName { get; set; }
        public string MediaLocation { get; set; }
        public string AvailabilitySetName { get; private set; }

        [Traverse]
        public List<ResourceExtensionReference> ResourceExtensionReferences { get; private set; }

        [Traverse]
        public List<DataVirtualHardDisk> DataVirtualHardDisks { get; private set; }

        [Traverse]
        public OSVirtualHardDisk OSVirtualHardDisk { get; private set; }
        public RoleSize RoleSize { get; private set; }
        public bool ProvisionGuestAgent { get; private set; }

        [Traverse]
        public VImageInput VmImageInput { get; private set; }

        [Ignore]
        internal bool OsVersion { get; set; }

        
    }
}