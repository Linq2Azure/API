namespace Linq2Azure.VirtualMachines
{
    public interface ILinuxConfigurationSetBuilder
    {
        ILinuxConfigurationSetBuilder WithComputerName(string name);
        ILinuxConfigurationSetBuilder WithHostname(string hostname);
        ILinuxConfigurationSetBuilder WithUserName(string username);
        ILinuxConfigurationSetBuilder WithUserPassword(string password);
        ILinuxConfigurationSetBuilder WithSSHEnabled();
        INetworkConfigurationSetBuilder AddNetworkConfiguration();
        IRoleBuilder AddRole(string roleName);
        IVirtualMachineBuilder FinalizeRoles();
    }
}