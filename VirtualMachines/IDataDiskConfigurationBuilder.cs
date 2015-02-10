namespace Linq2Azure.VirtualMachines
{
    public interface IDataDiskConfigurationBuilder
    {
        ISpecificDataDiskConfigurationBuilder AddDisk();
        IRoleBuilder FinsishedDataDiskConfiguration();
    }
}