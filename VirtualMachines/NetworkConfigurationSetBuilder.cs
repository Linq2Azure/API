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
                    new XElement("ConfigurationSetType", ConfigurationSet.ConfigurationSetType.ToString())
                );

            if (ConfigurationSet.InputEndpoints.Any())
            {
                var inputEndpoints = new XElement(XmlNamespaces.WindowsAzure + "InputEnpoints");

                foreach (var input in ConfigurationSet.InputEndpoints)
                {
                    var inputEndpoint = new XElement(XmlNamespaces.WindowsAzure + "InputEnpoint",
                            new XElement("LocalPort",input.LocalPort),
                            new XElement("Port", input.LocalPort),
                            new XElement("Protocol", input.Protocol.ToString()),
                            new XElement("Name", input.Name)
                        );
                    inputEndpoints.Add(inputEndpoint);
                }

                element.Add(inputEndpoints);
            }

            return element;
        }
    }
}