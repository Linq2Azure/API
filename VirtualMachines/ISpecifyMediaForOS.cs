namespace Linq2Azure.VirtualMachines
{
    public interface ISpecifyMediaForOS
    {
        ISpecifyOperatingSystem WithOSMedia(Os operatingSystem, OsDriveBlobStoredAt operatingSystemLocation);
        ISpecifyOperatingSystem WithImageMedia(ImageName image, OsDriveBlobStoredAt operatingSystemDriveBlobStoredAt);
    }
}