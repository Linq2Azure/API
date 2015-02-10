using System.Threading.Tasks;
using Linq2Azure.CloudServices;

namespace Linq2Azure.VirtualMachines
{
    public class VirtualMachineBuilder : IVirtualMachineBuilder, IRoleBuilder, IWindowsConfigurationSetBuilder, INetworkConfigurationSetBuilder, ILinuxConfigurationSetBuilder, IRoleOSVirtualHardDisk, IDataDiskConfigurationBuilder, ISpecificDataDiskConfigurationBuilder

    {
        private readonly CloudService _cloudService;
        private ConfigurationSet CurrentConfigurationSet;
        private Role CurrentRole { get; set; }
        public Deployment Deployment { get; set; }

        public VirtualMachineBuilder(CloudService cloudService,  string deploymentName)
        {
            _cloudService = cloudService;
            Deployment = new Deployment
            {
                DeploymentSlot = "Production",
                Name = deploymentName,
                Label = deploymentName
            };
        }

        public ILinuxConfigurationSetBuilder WithComputerName(string name)
        {
            CurrentConfigurationSet.ComputerName = name;
            return this;
        }

        public ILinuxConfigurationSetBuilder WithHostname(string hostname)
        {
            CurrentConfigurationSet.HostName = hostname;
            return this;
        }

        public ILinuxConfigurationSetBuilder WithUserName(string username)
        {
            CurrentConfigurationSet.UserName = username;
            return this;
        }

        public ILinuxConfigurationSetBuilder WithUserPassword(string password)
        {
            CurrentConfigurationSet.UserPassword = password;
            return this;
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
            var response = await client.PostAsync(new VirtualMachinePayloadBuilder(Deployment).CreatePostPayload());
            await _cloudService.Subscription.WaitForOperationCompletionAsync(response);
        }

        private AzureRestClient GetRestClient()
        {
            var servicePath = "services/hostedservices/" + _cloudService.Name + "/deployments";
            var client = _cloudService.Subscription.GetDatabaseRestClient(servicePath);
            return client;
        }

        public IRoleBuilder WithSize(string size)
        {
            CurrentRole.RoleSize = size;
            return this;
        }

        public IRoleBuilder WithVMImageName(string name)
        {
            CurrentRole.VMImageName = name;
            return this;
        }

        public IRoleBuilder WithMediaLocation(string location)
        {
            CurrentRole.MediaLocation = location;
            return this;
        }

        public IRoleOSVirtualHardDisk WithOSHardDisk(string label)
        {
            CurrentRole.OsVirtualHardDisk = new OSVirtualHardDisk
            {
                DiskLabel = label
            };
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
                EnableAutomaticUpdates = true,
                ComputerName = Deployment.Name,
                ConfigurationSetType = ConfigurationSetType.WindowsProvisioningConfiguration
            };
            return this;
        }

        public ILinuxConfigurationSetBuilder AddLinuxConfiguration()
        {
            if (CurrentConfigurationSet != null)
            {
                CurrentRole.ConfigurationSets.Add(CurrentConfigurationSet);
                CurrentConfigurationSet = null;
            }

            CurrentConfigurationSet = new ConfigurationSet
            {
                ConfigurationSetType = ConfigurationSetType.LinuxProvisioningConfiguration
            };
            return this;
        }

        public IVirtualMachineBuilder FinalizeRoles()
        {

            if (CurrentRole != null)
            {

                if (CurrentConfigurationSet != null)
                {
                    CurrentRole.ConfigurationSets.Add(CurrentConfigurationSet);
                    CurrentConfigurationSet = null;
                }

                Deployment.RoleList.Add(CurrentRole);
                CurrentRole = null;
            }

            return this;
        }

        public IDataDiskConfigurationBuilder WithDataDiskConfiguration(string label)
        {
            throw new System.NotImplementedException();
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

        public IRoleOSVirtualHardDisk WithDiskName(string name)
        {
            CurrentRole.OsVirtualHardDisk.DiskName = name;
            return this;
        }

        public IRoleOSVirtualHardDisk WithMediaLink(string link)
        {
            CurrentRole.OsVirtualHardDisk.MediaLink = link;
            return this;
        }

        public IRoleOSVirtualHardDisk WithSourceImageName(string name)
        {
            CurrentRole.OsVirtualHardDisk.SourceImageName = name;
            return this;
        }

        public IRoleBuilder Continue()
        {
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskName(string name)
        {
            CurrentRole.DataVirtualHardDisks[CurrentRole.DataVirtualHardDisks.Count - 1].DiskName = name;
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskHostCaching(HostCaching caching)
        {
            CurrentRole.DataVirtualHardDisks[CurrentRole.DataVirtualHardDisks.Count - 1].HostCaching = caching;
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskLabel(string label)
        {
            CurrentRole.DataVirtualHardDisks[CurrentRole.DataVirtualHardDisks.Count - 1].DiskLabel = label;
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskLun(int lun)
        {
            CurrentRole.DataVirtualHardDisks[CurrentRole.DataVirtualHardDisks.Count - 1].Lun = lun;
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskLogicalSizeInGB(int size)
        {
            CurrentRole.DataVirtualHardDisks[CurrentRole.DataVirtualHardDisks.Count - 1].LogicalDiskSizeInGB = size;
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskMediaLink(string link)
        {
            CurrentRole.DataVirtualHardDisks[CurrentRole.DataVirtualHardDisks.Count - 1].MediaLink = link;
            return this;
        }

        public IDataDiskConfigurationBuilder FinishedAddingDataDisk()
        {
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder AddDisk()
        {
            CurrentRole.DataVirtualHardDisks.Add(new DataVirtualHardDisk());
            return this;
        }

        public IRoleBuilder FinsishedDataDiskConfiguration()
        {
            return this;
        }
    }
}