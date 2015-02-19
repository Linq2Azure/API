using System;
using System.Linq;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class RoleXmlBuilder
    {
        public Role Role { get; set; }

        public RoleXmlBuilder(Role role)
        {
            Role = role;
        }

        public XElement Create()
        {

            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            var roleElement = new XElement(XmlNamespaces.WindowsAzure + "Role",new XAttribute(xsi + "type", "PersistentVMRole"),
                new XElement(XmlNamespaces.WindowsAzure + "RoleName", Role.RoleName));

            if(Role.OsVersion)
                roleElement.Add(new XElement("OsVersion", new XAttribute(xsi + "nil", true)));

            roleElement.Add(new XElement(XmlNamespaces.WindowsAzure + "RoleType", Role.RoleType));

            AddConfigurationSets(roleElement);
            AddResourceExtensionReferences(roleElement);
            AddAvailabilitySet(roleElement);
            AddDataDisks(roleElement);
            AddOSDisk(roleElement);

            roleElement.Add(new XElement(XmlNamespaces.WindowsAzure + "RoleSize", Role.RoleSize.ToString()));
            roleElement.Add(new XElement(XmlNamespaces.WindowsAzure + "ProvisionGuestAgent", Role.ProvisionGuestAgent));

            return roleElement;
        }

        private void AddResourceExtensionReferences(XElement roleElement)
        {
            if (!Role.ResourceExtensionReferences.Any())
                return;

            var references = new XElement(XmlNamespaces.WindowsAzure + "ResourceExtensionReferences");

            foreach (var reference in Role.ResourceExtensionReferences)
                references.Add(new ResourceExtensionReferenceXmlBuilder(reference).Create());

            roleElement.Add(references);
        }

        private void AddAvailabilitySet(XElement roleElement)
        {
            if (!String.IsNullOrEmpty(Role.AvailabilitySetName))
                roleElement.Add(new XElement(XmlNamespaces.WindowsAzure + "AvailabilitySetName", Role.AvailabilitySetName));
        }

        private void AddOSDisk(XElement roleElement)
        {
            if (!String.IsNullOrEmpty(Role.OSVirtualHardDisk.DiskLabel))
                roleElement.Add(new OSVirtualHardDiskXmlBuilder(Role.OSVirtualHardDisk, Role.OsVersion).Create());
        }

        private void AddDataDisks(XElement roleElement)
        {
            if (Role.DataVirtualHardDisks.Any())
            {
                var dataVirtualHardDisksElement = new XElement(XmlNamespaces.WindowsAzure + "DataVirtualHardDisks");

                foreach (var hd in Role.DataVirtualHardDisks)
                {
                    dataVirtualHardDisksElement.Add(new DataVirtualHardDiskXmlBuilder(hd).Create());
                }

                roleElement.Add(dataVirtualHardDisksElement);
            }
        }

        private void AddConfigurationSets(XElement roleElement)
        {
            if (Role.ConfigurationSets.Any())
            {
                var configurationSetsElement = new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSets");

                foreach (var cfg in Role.ConfigurationSets)
                {
                    configurationSetsElement.Add(BuildConfigurationSet(cfg));
                }

                roleElement.Add(configurationSetsElement);
            }
        }


        private XElement BuildConfigurationSet(ConfigurationSet cfg)
        {
            return new ConfigurationSetXmlFactory(cfg).Create();
        }
    }
}