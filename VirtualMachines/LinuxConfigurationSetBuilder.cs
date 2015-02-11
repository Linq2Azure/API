using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class LinuxConfigurationSetBuilder : IConfigurationSetBuilder
    {

        public ConfigurationSet ConfigurationSet { get; private set; }

        public LinuxConfigurationSetBuilder(ConfigurationSet configurationSet)
        {
            ConfigurationSet = configurationSet;
        }

        public XElement Create()
        {

            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            var element = new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSet", new XAttribute(xsi + "type", "LinuxProvisioningConfigurationSet"),
                new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSetType", ConfigurationSet.ConfigurationSetType.ToString()),
                new XElement(XmlNamespaces.WindowsAzure + "HostName", ConfigurationSet.HostName),
                new XElement(XmlNamespaces.WindowsAzure + "UserName", ConfigurationSet.UserName),
                new XElement(XmlNamespaces.WindowsAzure + "UserPassword", ConfigurationSet.UserPassword),
                new XElement(XmlNamespaces.WindowsAzure + "DisableSshPasswordAuthentication", ConfigurationSet.DisableSshPasswordAuthentication)
                );

            return element;
        }
    }
}