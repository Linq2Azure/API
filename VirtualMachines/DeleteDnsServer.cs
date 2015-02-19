using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Linq2Azure.CloudServices;

namespace Linq2Azure.VirtualMachines
{
    internal class DeleteDnsServer
    {
        internal async Task AddDnsServerAsync(CloudService cloudService, string deploymentName, DnsServer dnsServer)
        {

            Contract.Requires(!String.IsNullOrEmpty(deploymentName));

            var response = await GetRestClient(cloudService, deploymentName, dnsServer.Name ).DeleteAsync();
            await cloudService.Subscription.WaitForOperationCompletionAsync(response);
        }

        AzureRestClient GetRestClient(CloudService cloudService, string deploymentName, string pathSuffix = null)
        {
            var servicePath = "services/hostedservices/" + cloudService.Name + "/deployments/" + deploymentName + "/dnsservers/";
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return cloudService.Subscription.GetDatabaseRestClient(servicePath);
        }
    }
}