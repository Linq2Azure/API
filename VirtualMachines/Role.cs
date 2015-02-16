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
        }

        public Role(string roleType, string roleName, RoleSize roleSize, VirtualMachineBuilder vmBuilder)
        {
            Contract.Requires(!String.IsNullOrEmpty(roleType));
            Contract.Requires(!String.IsNullOrEmpty(roleName));
            Contract.Requires(vmBuilder != null);

            RoleType = roleType;
            RoleName = roleName;
            RoleSize = roleSize;
            VirtualMachineBuilder = vmBuilder;
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

        private AzureRestClient GetRestClient(string suffix = "", string queryString = "")
        {
            var cloudService = Deployment.GetCloudService();
            var servicePath = "services/hostedservices/" + cloudService.Name + "/deployments/" + suffix + queryString;
            var client = cloudService.Subscription.GetDatabaseRestClient(servicePath);
            return client;
        }

        [Ignore]
        public IDeployment Deployment { get; private set; }

        [Ignore]
        public VirtualMachineBuilder VirtualMachineBuilder { get; private set; }
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
        public string AvailabilitySetName { get; set; }

        [Traverse]
        public List<ResourceExtensionReference> ResourceExtensionReferences { get; set; }

        [Traverse]
        public List<DataVirtualHardDisk> DataVirtualHardDisks { get; set; }

        [Traverse]
        public OSVirtualHardDisk OSVirtualHardDisk { get; set; }
        public RoleSize RoleSize { get; set; }
        public bool ProvisionGuessAgent { get; set; }

        [Traverse]
        public VImageInput VmImageInput { get; set; }

        [Ignore]
        public bool OsVersion { get; set; }
    }
}