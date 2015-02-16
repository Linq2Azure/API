using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Linq2Azure.CloudServices;

namespace Linq2Azure.VirtualMachines
{
    public class Deployment : IDeployment
    {
        public Deployment()
        {
            RoleList = new List<Role>();
        }

        public Deployment(CloudService cloudService) : this()
        {
            CloudService = cloudService;
        }

        public Deployment(CloudService cloudService, DeploymentSlot slot, string name, string label) : this(cloudService)
        {
            Contract.Requires(cloudService != null);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(!String.IsNullOrEmpty(label));

            Slot = slot;
            Name = name;
            Label = label;
        }

        public Lazy<bool> IsVirtualMachineDeployment
        {
            get
            {
                return new Lazy<bool>(() => RoleList.Any(x => x.IsVirtualMachineRole()));
            }
        }

        public Task AddDnsServerAsync(DnsServer dnsServer)
        {
           return new AddDnsServer().AddDnsServerAsync(CloudService, Name, dnsServer);
        }

        public Task DeleteDnsServerAsync(DnsServer dnsServer)
        {
            return new DeleteDnsServer().AddDnsServerAsync(CloudService, Name, dnsServer);
        }

        public CloudService GetCloudService()
        {
            return CloudService;
        }

        public CloudService CloudService { get; private set; }
        public string Name { get; private set; }
        public DeploymentSlot Slot { get; private set; }
        public string Label { get; private set; }
        public List<Role> RoleList { get; private set; }

        [Traverse]
        public Dns Dns { get; private set; }
        public string VirtualNetworkName { get; private set; }
        public string ReservedIPName { get; private set; }
    }
}