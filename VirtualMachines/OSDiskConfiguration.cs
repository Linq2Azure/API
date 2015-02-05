namespace Linq2Azure.VirtualMachines
{
    public class OSDiskConfiguration
    {
        public string Name { get; set; }
        public HostCaching HostCaching { get; set; }
        public string OSState { get; set; }
        public string OS { get; set; }
        public string MediaLink { get; set; }
        public string LogicalDiskSizeInGB { get; set; }
        public string IOType { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, OS: {1}, LogicalDiskSizeInGB: {2}", Name, OS, LogicalDiskSizeInGB);
        }
    }
}