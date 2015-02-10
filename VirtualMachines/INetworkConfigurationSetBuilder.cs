namespace Linq2Azure.VirtualMachines
{
    public interface INetworkConfigurationSetBuilder
    {
        INetworkConfigurationSetBuilder AddWebPort();
        INetworkConfigurationSetBuilder AddRemoteDesktop();
        IRoleBuilder AddRole(string roleName);
        IVirtualMachineBuilder FinalizeRoles();
    }
}