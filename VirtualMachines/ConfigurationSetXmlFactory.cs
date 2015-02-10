using System;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class ConfigurationSetXmlFactory
    {

        private IConfigurationSetBuilder _builder;

        public ConfigurationSetXmlFactory(ConfigurationSet cfg)
        {
            switch (cfg.ConfigurationSetType)
            {
                case ConfigurationSetType.WindowsProvisioningConfiguration:
                    _builder = new WindowsConfigurationSetBuilder(cfg);
                    break;
                case ConfigurationSetType.LinuxProvisioningConfiguration:
                    _builder = new LinuxConfigurationSetBuilder(cfg);
                    break;
                case ConfigurationSetType.NetworkConfiguration:
                    _builder = new NetworkConfigurationSetBuilder(cfg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public XElement Create()
        {
            return _builder.Create();
        }
    }
}