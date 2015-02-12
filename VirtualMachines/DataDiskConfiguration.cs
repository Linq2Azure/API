namespace Linq2Azure.VirtualMachines
{
    public class DataDiskConfiguration
    {

        public DataDiskConfiguration()
        {
            HostCaching = HostCaching.None;
            Lun = 0;
            LogicalDiskSizeInGB = 1;
        }

        public string Name { get; set; }
        public HostCaching HostCaching { get; set; }
        public int Lun { get; set; }
        public string MediaLink { get; set; }
        public int LogicalDiskSizeInGB { get; set; }
        public string IOType { get; set; }
    }
}