namespace Linq2Azure.VirtualMachines
{
    public class DataVirtualHardDisk
    {

        public DataVirtualHardDisk()
        {
            HostCaching = HostCaching.None;
        }

        public HostCaching HostCaching { get; set; }
        public string DiskLabel { get; set; }
        public string DiskName { get; set; }
        public int? Lun { get; set; }
        public long LogicalDiskSizeInGB { get; set; }
        public string MediaLink { get; set; }
        public string SourceMediaLink { get; set; }
    }
}