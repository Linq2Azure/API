namespace Linq2Azure.VirtualMachines
{
    public interface ILinuxConfigurationGuided
    {
        INetworkConfigurationSetBuilder AddNetworkConfiguration();
    }
}