namespace Linq2Azure.VirtualMachines
{
    public class AdditionalDiskSettings
    {
        public HostCaching? HostCaching { get; set; }
        public int Lun { get; set; }
        public int SizeInGB { get; set; }
    }
}