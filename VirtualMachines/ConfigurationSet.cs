using System.Collections.Generic;

namespace Linq2Azure.VirtualMachines
{
    public class ConfigurationSet
    {
        public ConfigurationSet()
        {
            InputEndpoints = new List<InputEndpoint>();
            DisableSshPasswordAuthentication = true;
        }

        public ConfigurationSetType ConfigurationSetType { get; set; }
        public string ComputerName { get; set; }
        public string AdminPassword { get; set; }
        public bool EnableAutomaticUpdates { get; set; }
        public string TimeZone { get; set; }
        public DomainJoin DomainJoin { get; set; }
        public StoredCertificateSettings StoredCertificateSettings { get; set; }
        public WinRM WinRM { get; set; }
        public string AdminUsername { get; set; }
        public string CustomData { get; set; }
        public AdditionalUnattendedContent AdditionalUnattendedContent { get; set; }
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public bool DisableSshPasswordAuthentication { get; set; }
        public SSH SSH { get; set; }
        public List<InputEndpoint> InputEndpoints { get; set; }
        public List<string> SubnetNames { get; set; }
        public string StaticVirtualNetworkIPAddress { get; set; }
        public List<PublicIP> PublicIPs { get; set; } 
    }
}