using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using Linq2Azure.TrafficManagement;

namespace Linq2Azure.CloudServices
{
    public class AvailableExtensionImage
    {

        internal AvailableExtensionImage(XElement xml, Subscription subscription)
        {
            Subscription = subscription;
            var azureNamespace = XmlNamespaces.WindowsAzure;

            xml.HydrateObject(azureNamespace, this);

            PublicConfigurationSchema = PublicConfigurationSchema.FromBase64String();
            PrivateConfigurationSchema = PrivateConfigurationSchema.FromBase64String();
            SampleConfig = SampleConfig.FromBase64String();

            Versions = new LatentSequence<ExtensionImageVersion>(GetExtensionImageVersionsAsync);
        }

        public string ProviderNameSpace { get; private set; }
        public string Type { get; private set; }
        public string Version { get; private set; }
        public string Label { get; private set; }
        public string Description { get; private set; }
        public string HostingResources { get; private set; }
        public string ThumbprintAlgorithm { get; private set; }
        public string PublicConfigurationSchema { get; private set; }
        public string PrivateConfigurationSchema { get; private set; }
        public string SampleConfig { get; private set; }
        public bool ReplicationCompleted { get; private set; }
        public Uri Eula { get; private set; }
        public Uri PrivacyUri { get; private set; }
        public Uri HomepageUri { get; private set; }
        public bool IsJsonExtension { get; private set; }
        public string CompanyName { get; private set; }
        public string SupportedOS { get; private set; }
        public DateTimeOffset? PublishedDate { get; private set; }
        public Subscription Subscription { get; private set; }
        public LatentSequence<ExtensionImageVersion> Versions { get; private set; }

        private async Task<ExtensionImageVersion[]> GetExtensionImageVersionsAsync()
        {
            var xe = await GetRestClient("/" + Type).GetXmlAsync();
            return xe.Elements(XmlNamespaces.WindowsAzure + "ExtensionImage").
                Select(x => new ExtensionImageVersion(x))
                .ToArray();
        }

        internal AzureRestClient GetRestClient(string pathSuffix = null)
        {
            Contract.Requires(Subscription != null);

            var servicePath = "services/extensions/" + ProviderNameSpace;
            if (!string.IsNullOrEmpty(pathSuffix))
            {
                servicePath += pathSuffix;
            }
            return Subscription.GetCoreRestClient20140601(servicePath);
        }
    }
}