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
                    new XElement("ConfigurationSetType", ConfigurationSet.ConfigurationSetType.ToString()),
                    new XElement("ComputerName", ConfigurationSet.ComputerName),
                    new XElement("AdminPassword", ConfigurationSet.AdminPassword),
                    new XElement("EnableAutomaticUpdates",ConfigurationSet.EnableAutomaticUpdates)
                );

            return element;
        }
    }
}