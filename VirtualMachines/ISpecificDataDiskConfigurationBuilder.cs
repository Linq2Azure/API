namespace Linq2Azure.VirtualMachines
{
    public interface ISpecificDataDiskConfigurationBuilder
    {
        IGuidedSpecificDataDiskConfiguration StoredAt(DriveStoredAt location);
    }
}