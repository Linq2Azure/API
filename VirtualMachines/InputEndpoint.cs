namespace Linq2Azure.VirtualMachines
{
    public class InputEndpoint
    {
        public string LoadBalancedEndpointSetName { get; set; }
        public int LocalPort { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }

        [Traverse]
        public LoadBalancerProbe LoadBalancerProbe { get; set; }
        public Protocol Protocol { get; set; }
        public bool EnableDirectServerReturn { get; set; }
        public string LoadBalancerName { get; set; }
        public int? IdleTimeoutInMinutes { get; set; }
    }
}