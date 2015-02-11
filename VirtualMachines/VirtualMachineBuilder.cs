using System.Linq;
using System.Threading.Tasks;
using Linq2Azure.CloudServices;

namespace Linq2Azure.VirtualMachines
{
    public class VirtualMachineBuilder : IVirtualMachineBuilder, IRoleBuilder, IWindowsConfigurationSetBuilder, INetworkConfigurationSetBuilder, ILinuxConfigurationSetBuilder, IRoleOSVirtualHardDisk, IDataDiskConfigurationBuilder, ISpecificDataDiskConfigurationBuilder
    {
        private readonly CloudService _cloudService;
        public Deployment Deployment { get; set; }

        private Role GetCurrentRole()
        {
            if (!Deployment.RoleList.Any())
                return null;

            return Deployment.RoleList[Deployment.RoleList.Count - 1];
        }

        private ConfigurationSet GetCurrentConfigurationSet()
        {
            var currentRole = GetCurrentRole();

            if (currentRole == null)
                return null;

            if (!currentRole.ConfigurationSets.Any())
                return null;

            return currentRole.ConfigurationSets[currentRole.ConfigurationSets.Count - 1];
        }

        private DataVirtualHardDisk GetCurrentDataDisk()
        {

            var currentRole = GetCurrentRole();

            if (currentRole == null)
                return null;

            if (!currentRole.DataVirtualHardDisks.Any())
                return null;

            return Deployment.RoleList[Deployment.RoleList.Count - 1].DataVirtualHardDisks[currentRole.DataVirtualHardDisks.Count - 1];
        }

        public VirtualMachineBuilder(CloudService cloudService, string deploymentName)
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
            GetCurrentConfigurationSet().ComputerName = name;
            return this;
        }

        public ILinuxConfigurationSetBuilder WithHostname(string hostname)
        {
            GetCurrentConfigurationSet().HostName = hostname;
            return this;
        }

        public ILinuxConfigurationSetBuilder WithUserName(string username)
        {
            GetCurrentConfigurationSet().UserName = username;
            return this;
        }

        public ILinuxConfigurationSetBuilder WithUserPassword(string password)
        {
            GetCurrentConfigurationSet().UserPassword = password;
            return this;
        }

        public INetworkConfigurationSetBuilder AddPowershell()
        {
            GetCurrentConfigurationSet().InputEndpoints.Add(new InputEndpoint
            {
                LocalPort = 5986,
                Port = 5986,
                Name = "PowerShell",
                Protocol = Protocol.TCP
            });
            return this;
        }

        public INetworkConfigurationSetBuilder AddCustomPort(string name, Protocol protocol, int localPort, int remotePort)
        {
            GetCurrentConfigurationSet().InputEndpoints.Add(new InputEndpoint
            {
                LocalPort = localPort,
                Port = remotePort,
                Name = name,
                Protocol = protocol
            });
            return this;
        }

        public INetworkConfigurationSetBuilder AddCustomPort(string name, Protocol protocol, int localPort)
        {
            return AddCustomPort(name, protocol, localPort, localPort);
        }

        public IWindowsConfigurationSetBuilder AdminUsername(string username)
        {
            GetCurrentConfigurationSet().AdminUsername = username;
            return this;
        }

        public IRoleBuilder AddRole(string roleName)
        {

            var role = new Role
            {
                RoleName = roleName,
                RoleType = "PersistentVMRole"
            };

            Deployment.RoleList.Add(role);

            return this;
        }

        public async Task Provision()
        {
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
            GetCurrentRole().RoleSize = size;
            return this;
        }

        public IRoleBuilder WithVMImageName(string name)
        {
            GetCurrentRole().VMImageName = name;
            return this;
        }

        public IRoleBuilder WithMediaLocation(string location)
        {
            GetCurrentRole().MediaLocation = location;
            return this;
        }

        public IRoleBuilder WithOsVerion()
        {
            GetCurrentRole().OsVersion = true;
            return this;
        }

        public IRoleOSVirtualHardDisk WithOSHardDisk(string label)
        {
            GetCurrentRole().OsVirtualHardDisk = new OSVirtualHardDisk
            {
                DiskLabel = label
            };
            return this;
        }

        public IWindowsConfigurationSetBuilder AddWindowsConfiguration()
        {

            GetCurrentRole().ConfigurationSets.Add(new ConfigurationSet
            {
                EnableAutomaticUpdates = true,
                ComputerName = Deployment.Name,
                ConfigurationSetType = ConfigurationSetType.WindowsProvisioningConfiguration
            });
            return this;
        }

        public ILinuxConfigurationSetBuilder AddLinuxConfiguration()
        {
            GetCurrentRole().ConfigurationSets.Add(new ConfigurationSet
            {
                ConfigurationSetType = ConfigurationSetType.LinuxProvisioningConfiguration
            });
            return this;
        }

        public IVirtualMachineBuilder FinalizeRoles()
        {
            return this;
        }

        public IDataDiskConfigurationBuilder WithDataDiskConfiguration(string label)
        {
            return this;
        }

        public INetworkConfigurationSetBuilder AddNetworkConfiguration()
        {

            GetCurrentRole().ConfigurationSets.Add(new ConfigurationSet
            {
                ConfigurationSetType = ConfigurationSetType.NetworkConfiguration
            });
            return this;
        }

        public ILinuxConfigurationSetBuilder WithSSHEnabled()
        {
            GetCurrentConfigurationSet().DisableSshPasswordAuthentication = false;
            return this;
        }

        public IWindowsConfigurationSetBuilder EnableAutomaticUpdates(bool enable)
        {
            GetCurrentConfigurationSet().EnableAutomaticUpdates = enable;
            return this;
        }

        public IWindowsConfigurationSetBuilder ResetPasswordOnFirstLogon(bool reset)
        {
            GetCurrentConfigurationSet().EnableAutomaticUpdates = reset;
            return this;
        }

        public IWindowsConfigurationSetBuilder ComputerName(string name)
        {
            GetCurrentConfigurationSet().ComputerName = name;
            return this;
        }

        public IWindowsConfigurationSetBuilder AdminPassword(string password)
        {
            GetCurrentConfigurationSet().AdminPassword = password;
            return this;
        }

        public INetworkConfigurationSetBuilder AddRemoteDesktop()
        {
            GetCurrentConfigurationSet().InputEndpoints.Add(new InputEndpoint
            {
                LocalPort = 3389,
                Port = 3389,
                Name = "RDP",
                Protocol = Protocol.TCP
            });
            return this;
        }

        public INetworkConfigurationSetBuilder AddWebPort()
        {
            GetCurrentConfigurationSet().InputEndpoints.Add(new InputEndpoint
            {
                LocalPort = 80,
                Port = 80,
                Name = "HTTP",
                Protocol = Protocol.TCP
            });
            return this;
        }

        public INetworkConfigurationSetBuilder AddSSH()
        {
            GetCurrentConfigurationSet().InputEndpoints.Add(new InputEndpoint
            {
                LocalPort = 22,
                Port = 22,
                Name = "SSH",
                Protocol = Protocol.TCP
            });
            return this;
        }

        public IRoleOSVirtualHardDisk WithDiskName(string name)
        {
            GetCurrentRole().OsVirtualHardDisk.DiskName = name;
            return this;
        }

        public IRoleOSVirtualHardDisk WithMediaLink(string link)
        {
            GetCurrentRole().OsVirtualHardDisk.MediaLink = link;
            return this;
        }

        public IRoleOSVirtualHardDisk WithSourceImageName(string name)
        {
            GetCurrentRole().OsVirtualHardDisk.SourceImageName = name;
            return this;
        }

        public IRoleBuilder Continue()
        {
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskName(string name)
        {
            GetCurrentDataDisk().DiskName = name;
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskHostCaching(HostCaching caching)
        {
            GetCurrentDataDisk().HostCaching = caching;
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskLabel(string label)
        {
            GetCurrentDataDisk().DiskLabel = label;
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskLun(int lun)
        {
            GetCurrentDataDisk().Lun = lun;
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskLogicalSizeInGB(int size)
        {
            GetCurrentDataDisk().LogicalDiskSizeInGB = size;
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder WithDataDiskMediaLink(string link)
        {
            GetCurrentDataDisk().MediaLink = link;
            return this;
        }

        public IDataDiskConfigurationBuilder FinishedAddingDataDisk()
        {
            return this;
        }

        public ISpecificDataDiskConfigurationBuilder AddDisk()
        {
            GetCurrentRole().DataVirtualHardDisks.Add(new DataVirtualHardDisk());
            return this;
        }

        public IRoleBuilder FinsishedDataDiskConfiguration()
        {
            return this;
        }
    }
}