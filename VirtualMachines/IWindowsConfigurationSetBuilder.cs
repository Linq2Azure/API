namespace Linq2Azure.VirtualMachines
{
    public interface IWindowsConfigurationSetBuilder
    {
        IWindowsConfigurationSetBuilder EnableAutomaticUpdates(bool enable);
        IWindowsConfigurationSetBuilder ResetPasswordOnFirstLogon(bool reset);
        IWindowsConfigurationSetBuilder ComputerName(string name);
        IWindowsConfigurationSetBuilder AdminPassword(string password);
        IRoleBuilder AddRole(string roleName);
        IVirtualMachineBuilder FinalizeRoles();
        INetworkConfigurationSetBuilder AddNetworkConfiguration();
    }
}