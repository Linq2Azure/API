namespace Linq2Azure.VirtualMachines
{
    public class DataVirtualHardDisk
    {
        public HostCaching HostCaching { get; set; }
        public string DiskLabel { get; set; }
        public string DiskName { get; set; }
        public string Lun { get; set; }
        public long LogicalDiskSizeInGB { get; set; }
        public string MediaLink { get; set; }
        public string SourceMediaLink { get; set; }
    }
}