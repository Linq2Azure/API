using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class ResourceExtensionReference
    {

        public ResourceExtensionReference()
        {
            
        }

        public ResourceExtensionReference(XElement element,Subscription subscription)
        {
            Subscription = subscription;
            element.HydrateObject(XmlNamespaces.WindowsAzure,this);
        }

        public Subscription Subscription { get; private set; }
        public string Publisher { get; private set; }
        public string Name { get; private set; }
        public string Version { get; private set; }
        public string Label { get; private set; }
        public string Description { get; private set; }
        public string PublicConfigurationSchema { get; private set; }
        public string PrivateConfigurationSchema { get; private set; }
        public string SampleConfig { get; private set; }
        public bool ReplicationCompleted { get; private set; }
        public string Eula { get; private set; }
        public string PrivacyUri { get; private set; }
        public string HomepageUri { get; private set; }
        public bool IsJsonExtension { get; private set; }
        public bool IsInternalExtension { get; private set; }
        public bool DisallowMajorVersionUpgrade { get; private set; }
        public string CompanyName { get; private set; }
        public string SupportedOS { get; private set; }
        public string PublishedDate { get;private set; }
    }
}