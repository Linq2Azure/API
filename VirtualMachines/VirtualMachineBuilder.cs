using System;
using System.Threading.Tasks;
using Linq2Azure.CloudServices;

namespace Linq2Azure.VirtualMachines
{
    public class VirtualMachineBuilder : IVirtualMachineBuilder, IRoleBuilder, IWindowsConfigurationSetBuilder, INetworkConfigurationSetBuilder, 
                                         IDataDiskConfigurationBuilder, IOSDiskConfigurationBuilder
    {
        private readonly CloudService _cloudService;
        private readonly string _blobContainer;
        private readonly string _serviceName;
        private readonly string _password;
        private ConfigurationSet CurrentConfigurationSet;
        private Role CurrentRole { get; set; }
        public Deployment Deployment { get; set; }

        public VirtualMachineBuilder(CloudService cloudService, string blobContainer, string serviceName, string virtualMachineName, string password)
        {
            _cloudService = cloudService;
            _blobContainer = blobContainer;
            _serviceName = serviceName;
            _password = password;
            Deployment = new Deployment
            {
                DeploymentSlot = "Production",
                Name = virtualMachineName,
                Label = virtualMachineName
            };
        }

        public IRoleBuilder AddRole(string roleName)
        {

            if (CurrentRole != null)
            {
                Deployment.RoleList.Add(CurrentRole);

                if (CurrentConfigurationSet != null)
                {
                    CurrentRole.ConfigurationSets.Add(CurrentConfigurationSet);
                    CurrentConfigurationSet = null;
                }

                CurrentRole = null;
            }

            CurrentRole = new Role
            {
                RoleName = roleName,
                RoleType = "PersistentVMRole"
            };

            return this;
        }

        public async Task Provision()
        {
            if (CurrentRole != null)
            {
                Deployment.RoleList.Add(CurrentRole);

                if (CurrentConfigurationSet != null)
                {
                    CurrentRole.ConfigurationSets.Add(CurrentConfigurationSet);
                    CurrentConfigurationSet = null;
                }

                CurrentRole = null;
            }

            var client = GetRestClient();
            await client.PostAsync(new VirtualMachinePayloadBuilder(Deployment).CreatePostPayload());
           
        }

        private AzureRestClient GetRestClient()
        {
            var servicePath = "services/hostedservices/" + _cloudService.Name + "/deployments";
            return _cloudService.Subscription.GetDatabaseRestClient(servicePath);
        }

        public IRoleBuilder WithSize(string size)
        {
            CurrentRole.RoleSize = size;
            return this;
        }

        public IWindowsConfigurationSetBuilder AddWindowsConfiguration()
        {

            if (CurrentConfigurationSet != null)
            {
                CurrentRole.ConfigurationSets.Add(CurrentConfigurationSet);
                CurrentConfigurationSet = null;
            }

            CurrentConfigurationSet = new ConfigurationSet
            {
                AdminPassword = _password,
                EnableAutomaticUpdates = true,
                ComputerName = Deployment.Name,
                ConfigurationSetType = ConfigurationSetType.WindowsProvisioningConfiguration
            };
            return this;
        }

        public IVirtualMachineBuilder FinalizeRoles()
        {

            if (CurrentRole != null)
            {
                Deployment.RoleList.Add(CurrentRole);
                CurrentRole = null;
            }

            return this;
        }

        public INetworkConfigurationSetBuilder AddNetworkConfiguration()
        {

            if (CurrentConfigurationSet != null)
            {
                CurrentRole.ConfigurationSets.Add(CurrentConfigurationSet);
                CurrentConfigurationSet = null;
            }

            CurrentConfigurationSet = new ConfigurationSet
            {
                ConfigurationSetType = ConfigurationSetType.NetworkConfiguration
            };
            return this;
        }

        public IWindowsConfigurationSetBuilder EnableAutomaticUpdates(bool enable)
        {
            CurrentConfigurationSet.EnableAutomaticUpdates = enable;
            return this;
        }

        public IWindowsConfigurationSetBuilder ResetPasswordOnFirstLogon(bool reset)
        {
            CurrentConfigurationSet.EnableAutomaticUpdates = reset;
            return this;
        }

        public IWindowsConfigurationSetBuilder ComputerName(string name)
        {
            CurrentConfigurationSet.ComputerName = name;
            return this;
        }

        public IWindowsConfigurationSetBuilder AdminPassword(string password)
        {
            CurrentConfigurationSet.AdminPassword = password;
            return this;
        }

        public INetworkConfigurationSetBuilder AddWebPort()
        {
            CurrentConfigurationSet.InputEndpoints.Add(new InputEndpoint
            {
                LocalPort = "3389",
                Name = "RDP",
                Protocol = Protocol.TCP
            });
            return this;
        }

        public INetworkConfigurationSetBuilder AddRemoteDesktop()
        {
            CurrentConfigurationSet.InputEndpoints.Add(new InputEndpoint
            {
                LocalPort = "80",
                Name = "web",
                Protocol = Protocol.TCP
            });
            return this;
        }

        public IRoleBuilder AddDisk(HostCaching caching, string label, int size)
        {
            CurrentRole.DataVirtualHardDisks.Add(new DataVirtualHardDisk
            {
                HostCaching = caching,
                DiskLabel = label,
                LogicalDiskSizeInGB = size,
                MediaLink = GetVhdUri(_blobContainer, _serviceName, Deployment.Name, true).ToString()
            });
            return this;
        }

        private static Uri GetVhdUri(string blobcontainerAddress, string serviceName, string vmName, bool cacheDisk = false, bool https = false)
        {
            var now = DateTime.UtcNow;
            var dateString = now.Year + "-" + now.Month + "-" + now.Day;

            var address = string.Format("http{0}://{1}/{2}-{3}{4}-{5}-650.vhd", https ? "s" : string.Empty, blobcontainerAddress, serviceName, vmName, cacheDisk ? "-CacheDisk" : string.Empty, dateString);
            return new Uri(address);
        }

        public IRoleBuilder WithImage(string imageName)
        {
            CurrentRole.OsVirtualHardDisk = new OSVirtualHardDisk
            {
                MediaLink = GetVhdUri(_blobContainer, _serviceName, Deployment.Name).ToString(),
                SourceImageName = imageName
            };
            return this;
        }
    }
}