namespace Linq2Azure.VirtualMachines
{
    public interface IRoleOSVirtualHardDisk
    {
        IRoleOSVirtualHardDisk WithDiskName(string name);
        IRoleOSVirtualHardDisk WithMediaLink(string link);
        IRoleOSVirtualHardDisk WithSourceImageName(string name);
        IRoleBuilder Continue();
    }
}