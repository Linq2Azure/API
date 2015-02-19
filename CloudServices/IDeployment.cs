using System;
using System.Threading.Tasks;
using Linq2Azure.VirtualMachines;

namespace Linq2Azure.CloudServices
{
    public interface IDeployment
    {
        string Name { get; }
        string Label { get; }
        DeploymentSlot Slot { get; }
        Lazy<bool> IsVirtualMachineDeployment { get; }
        Task AddDnsServerAsync(DnsServer dnsServer);
        Task DeleteDnsServerAsync(DnsServer dnsServer);
        CloudService GetCloudService();
    }
}