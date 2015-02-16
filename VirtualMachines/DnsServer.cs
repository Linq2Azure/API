using System.Net;

namespace Linq2Azure.VirtualMachines
{
    public class DnsServer
    {

        public DnsServer()
        {
            
        }

        public DnsServer(string name, IPAddress address)
        {
            Name = name;
            Address = address;
        }

        public string Name { get; private set; }
        public IPAddress Address { get; private set; }
    }
}