using System;
using System.Diagnostics.Contracts;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class LinuxConfigurationSetBuilder : IConfigurationSetBuilder
    {

        public ConfigurationSet ConfigurationSet { get; private set; }

        public LinuxConfigurationSetBuilder(ConfigurationSet configurationSet)
        {
            Contract.Requires(configurationSet != null);
            ConfigurationSet = configurationSet;
        }

        public XElement Create()
        {

            Contract.Requires(!String.IsNullOrEmpty(ConfigurationSet.HostName));
            Contract.Requires(!String.IsNullOrEmpty(ConfigurationSet.UserName));
            Contract.Requires(!String.IsNullOrEmpty(ConfigurationSet.UserPassword));

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