using System.Collections.Generic;
namespace Linq2Azure.VirtualMachines
{
    public class ConfigurationSet
    {
        public ConfigurationSet()
        {
            InputEndpoints = new List<InputEndpoint>();
            SubnetNames = new List<string>();
            DisableSshPasswordAuthentication = true;
        }

        public ConfigurationSetType ConfigurationSetType { get; set; }
        public string ComputerName { get; set; }
        public string AdminPassword { get; set; }
        public bool EnableAutomaticUpdates { get; set; }
        public string TimeZone { get; set; }

        [Traverse]
        public DomainJoin DomainJoin { get; set; }

        [Traverse]
        public StoredCertificateSettings StoredCertificateSettings { get; set; }
        public WinRM WinRM { get; set; }
        public string AdminUsername { get; set; }
        public string CustomData { get; set; }

        [Traverse]
        public AdditionalUnattendedContent AdditionalUnattendedContent { get; set; }
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public bool DisableSshPasswordAuthentication { get; set; }
        public SSH SSH { get; set; }

        [Traverse]
        public List<InputEndpoint> InputEndpoints { get; set; }

        [Traverse]
        public List<string> SubnetNames { get; set; }
        public string StaticVirtualNetworkIPAddress { get; set; }

        [Traverse]
        public List<PublicIP> PublicIPs { get; set; }
        public bool ResetPasswordOnFirstLogin { get; set; }
    }
}