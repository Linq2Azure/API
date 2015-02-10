namespace Linq2Azure.VirtualMachines
{
    public interface ILinuxConfigurationSetBuilder
    {
        ILinuxConfigurationSetBuilder WithComputerName(string name);
        ILinuxConfigurationSetBuilder WithHostname(string hostname);
        ILinuxConfigurationSetBuilder WithUserName(string username);
        ILinuxConfigurationSetBuilder WithUserPassword(string password);
        IRoleBuilder AddRole(string roleName);
        IVirtualMachineBuilder FinalizeRoles();
        INetworkConfigurationSetBuilder AddNetworkConfiguration();
    }
}