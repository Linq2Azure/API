using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class ResourceExtensionReferenceXmlBuilder
    {
        public ResourceExtensionReference Reference { get; set; }

        public ResourceExtensionReferenceXmlBuilder(ResourceExtensionReference reference)
        {
            Reference = reference;
        }

        public XElement Create()
        {
            var element = new XElement(XmlNamespaces.WindowsAzure + "ResourceExtensionReference",
                new XElement(XmlNamespaces.WindowsAzure + "ReferenceName", Reference.Name),
                new XElement(XmlNamespaces.WindowsAzure + "Publisher", Reference.Publisher),
                new XElement(XmlNamespaces.WindowsAzure + "Name", Reference.Name),
                new XElement(XmlNamespaces.WindowsAzure + "Version", Reference.Version)
                );

            return element;
        }
    }
}