namespace Linq2Azure.VirtualMachines
{
    public class DataDiskConfiguration
    {
        public string Name { get; set; }
        public HostCaching HostCaching { get; set; }
        public string Lun { get; set; }
        public string MediaLink { get; set; }
        public string LogicalDiskSizeInGB { get; set; }
        public string IOType { get; set; }
    }
}