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
            var ns = XmlNamespaces.WindowsAzure;
            var roleListElement = new XElement(ns + "RoleList");
            var content = new XElement(ns + "Deployment",
                new XElement(ns + "Name", Deployment.Name),
                new XElement(ns + "DeploymentSlot", Deployment.DeploymentSlot),
                new XElement(ns + "Label", Deployment.Label),
                roleListElement);

            foreach (var role in Deployment.RoleList)
            {
                roleListElement.Add(BuildRole(role));
            }

            return content;
        }


        private XElement BuildRole(Role role)
        {
            var roleElement = new XElement(XmlNamespaces.WindowsAzure + "Role",
                new XElement("RoleName", role.RoleName),
                new XElement("RoleType", role.RoleType)
                );

            if (role.ConfigurationSets.Any())
            {
                var configurationSetsElement = new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSets");

                foreach (var cfg in role.ConfigurationSets)
                {
                    configurationSetsElement.Add(BuildConfigurationSet(cfg));
                }

            }

            return roleElement;
        }

        private XElement BuildConfigurationSet(ConfigurationSet cfg)
        {
            return new ConfigurationSetXmlFactory(cfg).Create();

        }
    }
}