namespace Linq2Azure.VirtualMachines
{
    public interface IRoleBuilder
    {
        IRoleBuilder WithSize(string size);
        IWindowsConfigurationSetBuilder AddWindowsConfiguration();
        IVirtualMachineBuilder FinalizeRoles();
    }
}