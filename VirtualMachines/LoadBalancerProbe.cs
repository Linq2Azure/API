namespace Linq2Azure.VirtualMachines
{
    public class LoadBalancerProbe
    {
        public string Path { get; set; }
        public string Port { get; set; }
        public string Protocol { get; set; }
        public int? IntervalInSeconds { get; set; }
        public int? TimeoutInSeconds { get; set; }
    }
}