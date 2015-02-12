namespace Linq2Azure.VirtualMachines
{
    public interface IRoleBuilder
    {
        IRoleOSVirtualHardDisk WithOSHardDisk(OperationSystemDiskLabel label);
    }
}