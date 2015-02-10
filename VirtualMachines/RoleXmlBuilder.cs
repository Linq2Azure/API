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
            var roleElement = new XElement(XmlNamespaces.WindowsAzure + "Role",
                new XElement(XmlNamespaces.WindowsAzure + "RoleName", Role.RoleName),
                new XElement(XmlNamespaces.WindowsAzure + "RoleType", Role.RoleType)
                );

            if (UsingVMImage())
            {
                roleElement.Add(new XElement(XmlNamespaces.WindowsAzure + "VMImageName", Role.VMImageName));
                roleElement.Add(new XElement(XmlNamespaces.WindowsAzure + "MediaLocation", Role.MediaLocation));
            }

            if (Role.ConfigurationSets.Any())
            {
                var configurationSetsElement = new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSets");

                foreach (var cfg in Role.ConfigurationSets)
                {
                    configurationSetsElement.Add(BuildConfigurationSet(cfg));
                }

                roleElement.Add(configurationSetsElement);
            }

            if (Role.DataVirtualHardDisks.Any() && !(UsingVMImage()))
            {
                var dataVirtualHardDisksElement = new XElement(XmlNamespaces.WindowsAzure + "DataVirtualHardDisks");

                foreach (var hd in Role.DataVirtualHardDisks)
                {
                    dataVirtualHardDisksElement.Add(new DataVirtualHardDiskXmlBuilder(hd).Create());
                }

                roleElement.Add(dataVirtualHardDisksElement);
            }

            if (!UsingVMImage())
            {
                roleElement.Add(new OSVirtualHardDiskXmlBuilder(Role.OsVirtualHardDisk).Create());
            }

            return roleElement;
        }

        private bool UsingVMImage()
        {
            return !String.IsNullOrEmpty(Role.VMImageName) || !String.IsNullOrEmpty(Role.MediaLocation);
        }

        private XElement BuildConfigurationSet(ConfigurationSet cfg)
        {
            return new ConfigurationSetXmlFactory(cfg).Create();
        }
    }
}