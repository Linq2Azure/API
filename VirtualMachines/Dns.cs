using System.Collections.Generic;

namespace Linq2Azure.VirtualMachines
{
    public class Dns
    {

        public Dns()
        {
            DnsServers = new List<DnsServer>();
        }

        public List<DnsServer> DnsServers { get; set; }
    }
}