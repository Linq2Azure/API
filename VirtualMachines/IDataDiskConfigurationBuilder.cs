namespace Linq2Azure.VirtualMachines
{
    public interface IDataDiskConfigurationBuilder
    {
        ISpecificDataDiskConfigurationBuilder Existing(string name);
        ISpecificDataDiskConfigurationBuilder IsNew();

    }
}