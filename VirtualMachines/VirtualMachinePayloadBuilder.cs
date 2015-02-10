using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class VirtualMachinePayloadBuilder
    {
        public Deployment Deployment { get; private set; }

        public VirtualMachinePayloadBuilder(Deployment deployment)
        {
            Deployment = deployment;
        }

        public XElement CreatePostPayload()
        {
            var roleListElement = new XElement(XmlNamespaces.WindowsAzure + "RoleList");
            var content = new XElement(XmlNamespaces.WindowsAzure + "Deployment");
                content.Add(
                new XElement(XmlNamespaces.WindowsAzure + "Name", Deployment.Name),
                new XElement(XmlNamespaces.WindowsAzure + "DeploymentSlot", Deployment.DeploymentSlot),
                new XElement(XmlNamespaces.WindowsAzure + "Label", Deployment.Label),
                roleListElement);

            content.Elements().ToList().ForEach(x => x.RemoveAttributes());

            foreach (var role in Deployment.RoleList)
                roleListElement.Add(new RoleXmlBuilder(role).Create());
            Debug.WriteLine(content);

            return content;
        }
    }
}