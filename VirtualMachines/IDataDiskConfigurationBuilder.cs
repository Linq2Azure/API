namespace Linq2Azure.VirtualMachines
{
    public interface IDataDiskConfigurationBuilder
    {
        IRoleBuilder AddDisk(HostCaching caching, string label, int size);
    }
}