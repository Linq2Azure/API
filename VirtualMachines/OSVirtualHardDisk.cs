namespace Linq2Azure.VirtualMachines
{
    public class OSVirtualHardDisk
    {

        public HostCaching? HostCaching { get; set; }
        public string DiskLabel { get; set; }
        public string DiskName { get; set; }
        public string MediaLink { get; set; }
        public string SourceImageName { get; set; }
        public string OS { get; set; }
        public string ResizedSizeInGB { get; set; }
        public string RemoteSourceImageLink { get; set; }
    }
}