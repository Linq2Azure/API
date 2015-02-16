using System.Linq;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class NetworkConfigurationSetBuilder : IConfigurationSetBuilder
    {
        public ConfigurationSet ConfigurationSet { get; private set; }

        public NetworkConfigurationSetBuilder(ConfigurationSet configurationSet)
        {
            ConfigurationSet = configurationSet;
        }

        public XElement Create()
        {

            var element = new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSet",
                    new XElement(XmlNamespaces.WindowsAzure + "ConfigurationSetType", ConfigurationSet.ConfigurationSetType.ToString()));

            if (ConfigurationSet.InputEndpoints.Any())
            {
                var inputEndpoints = new XElement(XmlNamespaces.WindowsAzure + "InputEndpoints");

                foreach (var input in ConfigurationSet.InputEndpoints)
                {
                    var inputEndpoint = new XElement(XmlNamespaces.WindowsAzure + "InputEndpoint",
                            new XElement(XmlNamespaces.WindowsAzure + "LocalPort", input.LocalPort),
                            new XElement(XmlNamespaces.WindowsAzure + "Name", input.Name),
                            new XElement(XmlNamespaces.WindowsAzure + "Port", input.Port),
                            new XElement(XmlNamespaces.WindowsAzure + "Protocol", input.Protocol.ToString().ToLower())
                            
                        );

                    inputEndpoints.Add(inputEndpoint);
                }

                element.Add(inputEndpoints);
            }

            return element;
        }
    }
}