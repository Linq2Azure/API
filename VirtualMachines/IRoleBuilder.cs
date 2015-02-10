namespace Linq2Azure.VirtualMachines
{
    public interface IRoleBuilder
    {
        IRoleBuilder WithSize(string size);
        IRoleBuilder WithVMImageName(string name);
        IRoleBuilder WithMediaLocation(string location);
        IRoleOSVirtualHardDisk WithOSHardDisk(string label);
        IWindowsConfigurationSetBuilder AddWindowsConfiguration();
        ILinuxConfigurationSetBuilder AddLinuxConfiguration();
        INetworkConfigurationSetBuilder AddNetworkConfiguration();
        IVirtualMachineBuilder FinalizeRoles();
        IDataDiskConfigurationBuilder WithDataDiskConfiguration(string label);
    }
}