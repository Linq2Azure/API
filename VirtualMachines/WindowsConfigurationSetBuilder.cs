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
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            var element = new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSet", new XAttribute(xsi + "type", "WindowsProvisioningConfigurationSet"),
                    new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSetType", ConfigurationSet.ConfigurationSetType.ToString()),
                    new XElement(XmlNamespaces.WindowsAzure + "ComputerName", ConfigurationSet.ComputerName),
                    new XElement(XmlNamespaces.WindowsAzure + "AdminPassword", ConfigurationSet.AdminPassword),
                    new XElement(XmlNamespaces.WindowsAzure + "AdminUsername", ConfigurationSet.AdminUsername),
                    new XElement(XmlNamespaces.WindowsAzure + "EnableAutomaticUpdates", ConfigurationSet.EnableAutomaticUpdates)
                );

            var winrm = new XElement(XmlNamespaces.WindowsAzure + "WinRM", 
                new XElement(XmlNamespaces.WindowsAzure + "Listeners",
                    new XElement(XmlNamespaces.WindowsAzure + "Listener",
                         new XElement(XmlNamespaces.WindowsAzure + "Type","Http"))));

            element.Add(winrm);

            return element;
        }
    }
}