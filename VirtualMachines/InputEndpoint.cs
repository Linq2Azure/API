namespace Linq2Azure.VirtualMachines
{
    public class InputEndpoint
    {
        public string InputEndpoint_ { get; set; }
        public string LoadBalancedEndpointSetName { get; set; }
        public string LocalPort { get; set; }
        public string Name { get; set; }
        public string Port { get; set; }
        public LoadBalancerProbe LoadBalancerProbe { get; set; }
        public Protocol Protocol { get; set; }
        public bool EnableDirectServerReturn { get; set; }
        public EndpointACL EndpointACL { get; set; }
        public string LoadBalancerName { get; set; }
        public int IdleTimeoutInMinutes { get; set; }
    }
}