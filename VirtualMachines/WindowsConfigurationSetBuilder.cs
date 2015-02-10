using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class WindowsConfigurationSetBuilder : IConfigurationSetBuilder
    {
        public ConfigurationSet ConfigurationSet { get; private set; }

        public WindowsConfigurationSetBuilder(ConfigurationSet configurationSet)
        {
            ConfigurationSet = configurationSet;
        }

        public XElement Create()
        {

            var element = new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSet",
                    new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSetType", ConfigurationSet.ConfigurationSetType.ToString()),
                    new XElement(XmlNamespaces.WindowsAzure + "ComputerName", ConfigurationSet.ComputerName),
                    new XElement(XmlNamespaces.WindowsAzure + "AdminUsername", "Administrator"),
                    new XElement(XmlNamespaces.WindowsAzure + "AdminPassword", ConfigurationSet.AdminPassword),
                    new XElement(XmlNamespaces.WindowsAzure + "EnableAutomaticUpdates", ConfigurationSet.EnableAutomaticUpdates)
                );

            return element;
        }
    }
}