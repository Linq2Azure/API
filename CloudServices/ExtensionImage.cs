using System;
using System.Xml.Linq;

namespace Linq2Azure.CloudServices
{
    public class ExtensionImage
    {
        internal ExtensionImage(XElement xml)
        {
            var azureNamespace = XmlNamespaces.WindowsAzure;

            xml.HydrateObject(azureNamespace, this);

            PublicConfigurationSchema = PublicConfigurationSchema.FromBase64String();
            PrivateConfigurationSchema = PrivateConfigurationSchema.FromBase64String();
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
    }
}