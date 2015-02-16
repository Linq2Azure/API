using System.Threading.Tasks;
using System.Xml.Linq;
using Linq2Azure.CloudServices;

namespace Linq2Azure.VirtualMachines
{
    internal class AddDnsServer
    {
        internal async Task AddDnsServerAsync(CloudService cloudService, string deploymentName, DnsServer dnsServer)
        {
            var content = new XElement(XmlNamespaces.WindowsAzure + "DnsServer", 
                    new XElement(XmlNamespaces.WindowsAzure + "Name", dnsServer.Name),
                    new XElement(XmlNamespaces.WindowsAzure + "Address", dnsServer.Address.ToString())
                );
            var response = await GetRestClient(cloudService, deploymentName).PostAsync(content);
            await cloudService.Subscription.WaitForOperationCompletionAsync(response);
        }

        AzureRestClient GetRestClient(CloudService cloudService, string deploymentName,  string pathSuffix = null)
        {
            var servicePath = "services/hostedservices/" + cloudService.Name + "/deployments/" + deploymentName + "/dnsservers";
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return cloudService.Subscription.GetDatabaseRestClient(servicePath);
        }
    }
}