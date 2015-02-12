using System.Threading.Tasks;

namespace Linq2Azure.VirtualMachines
{
    public interface INetworkConfigurationSetBuilder
    {
        INetworkConfigurationSetBuilder AddWebPort();
        INetworkConfigurationSetBuilder AddRemoteDesktop();
        INetworkConfigurationSetBuilder AddSSH();
        INetworkConfigurationSetBuilder AddPowershell();
        INetworkConfigurationSetBuilder AddCustomPort(string name, Protocol protocol, int localPort, int remotePort);
        INetworkConfigurationSetBuilder AddCustomPort(string name, Protocol protocol, int localPort);
        IRoleBuilder AddRole(string roleName, RoleSize roleSize);
        IDataDiskConfigurationBuilder AddDisk(DiskLabel label);
        Task Provision();
    }
}