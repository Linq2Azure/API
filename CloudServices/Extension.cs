using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.CloudServices
{
    public class Extension
    {
        public string ProviderNameSpace { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string Thumbprint { get; set; }
        public string ThumbprintAlgorithm { get; set; }
        public string PublicConfiguration { get; set; }
        public string PrivateConfiguration { get; set; }
        public bool IsJsonExtension { get; private set; }
        public string DisallowMajorVersionUpgrade { get; private set; }
        public CloudService Parent { get; private set; }

        public Extension()
        {}

        public Extension(XElement xml, CloudService parent)
        {
            Contract.Requires(parent != null);

            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);

            PublicConfiguration = PublicConfiguration.FromBase64String();
            PrivateConfiguration = PrivateConfiguration.FromBase64String();
            
            Parent = parent;
        }

        internal async Task AddAsync(CloudService parent)
        {
            var ns = XmlNamespaces.WindowsAzure;

            var content = new XElement(ns + "Extension", BuildExtensionXml(ns).ToArray());

            var response = await GetRestClient(parent).PostAsync(content);
            await parent.Subscription.WaitForOperationCompletionAsync(response);
            Parent = parent;
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Parent != null);
            var httpResponseMessage = await GetRestClient(Parent, "/" + Id).DeleteAsync();
            await Parent.Subscription.WaitForOperationCompletionAsync(httpResponseMessage);
            Parent = null;
        }

        private IEnumerable<object> BuildExtensionXml(XNamespace ns)
        {
            yield return new XElement(ns + "ProviderNameSpace", ProviderNameSpace);
            yield return new XElement(ns + "Type", Type);
            yield return new XElement(ns + "Id", Id);
            if (!string.IsNullOrWhiteSpace(Thumbprint))
            {
                yield return new XElement(ns + "Thumbprint", Thumbprint);
            }
            if (!string.IsNullOrWhiteSpace(ThumbprintAlgorithm))
            {
                yield return new XElement(ns + "ThumbprintAlgorithm", ThumbprintAlgorithm);
            }
            if (!string.IsNullOrWhiteSpace(PublicConfiguration))
            {
                yield return new XElement(ns + "PublicConfiguration", Convert.ToBase64String(Encoding.UTF8.GetBytes(PublicConfiguration)));
            }
            if (!string.IsNullOrWhiteSpace(PrivateConfiguration))
            {
                yield return new XElement(ns + "PrivateConfiguration", Convert.ToBase64String(Encoding.UTF8.GetBytes(PrivateConfiguration)));
            }
            yield return new XElement(
                ns + "Version",
                !string.IsNullOrWhiteSpace(Version)
                    ? Version
                    : "*");
        }

        private static AzureRestClient GetRestClient(
            CloudService parent,
            string pathSuffix = null)
        {
            var servicePath = "services/hostedServices/" + parent.Name + "/extensions";
            if (pathSuffix != null)
            {
                servicePath += pathSuffix;
            }
            return parent.Subscription.GetCoreRestClient20140601(servicePath);
        }
    }
}