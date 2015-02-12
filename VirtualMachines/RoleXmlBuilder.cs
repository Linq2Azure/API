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
            
            
            if (Role.ConfigurationSets.Any())
            {
                var configurationSetsElement = new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSets");

                foreach (var cfg in Role.ConfigurationSets)
                {
                    configurationSetsElement.Add(BuildConfigurationSet(cfg));
                }

                roleElement.Add(configurationSetsElement);
            }

            if (Role.DataVirtualHardDisks.Any())
            {
                var dataVirtualHardDisksElement = new XElement(XmlNamespaces.WindowsAzure + "DataVirtualHardDisks");

                foreach (var hd in Role.DataVirtualHardDisks)
                {
                    dataVirtualHardDisksElement.Add(new DataVirtualHardDiskXmlBuilder(hd).Create());
                }

                roleElement.Add(dataVirtualHardDisksElement);
            }

            if (!String.IsNullOrEmpty(Role.OsVirtualHardDisk.DiskLabel))
                roleElement.Add(new OSVirtualHardDiskXmlBuilder(Role.OsVirtualHardDisk, Role.OsVersion).Create());

            roleElement.Add(new XElement(XmlNamespaces.WindowsAzure + "RoleSize", Role.RoleSize.ToString()));

            return roleElement;
        }


        private XElement BuildConfigurationSet(ConfigurationSet cfg)
        {
            return new ConfigurationSetXmlFactory(cfg).Create();
        }
    }
}