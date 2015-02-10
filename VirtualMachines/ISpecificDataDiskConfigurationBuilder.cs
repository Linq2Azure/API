namespace Linq2Azure.VirtualMachines
{
    public interface ISpecificDataDiskConfigurationBuilder
    {
        ISpecificDataDiskConfigurationBuilder WithDataDiskName(string name);
        ISpecificDataDiskConfigurationBuilder WithDataDiskHostCaching(HostCaching caching);
        ISpecificDataDiskConfigurationBuilder WithDataDiskLabel(string label);
        ISpecificDataDiskConfigurationBuilder WithDataDiskLun(int lun);
        ISpecificDataDiskConfigurationBuilder WithDataDiskLogicalSizeInGB(int size);
        ISpecificDataDiskConfigurationBuilder WithDataDiskMediaLink(string link);
        IDataDiskConfigurationBuilder FinishedAddingDataDisk();
    }
}