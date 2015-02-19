namespace Linq2Azure.VirtualMachines
{
    public interface ISpecifyOperatingSystem
    {
        IWindowsConfigurationSetBuilder AddWindowsConfiguration(ComputerName computerName, Administrator administrator, Password password);
        ILinuxConfigurationSetBuilder AddLinuxConfiguration(Hostname hostname, Administrator administrator, Password password);
    }
}