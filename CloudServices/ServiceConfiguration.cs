using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Linq2Azure.CloudServices
{
    public class ServiceConfiguration
    {
        private readonly XElement _existingConfiguration;
        private readonly IEnumerable<RoleConfiguration> _roles;

        public string ServiceName { get; set; }
        public int OsFamily { get; set; }
        public string OsVersion { get; set; }

        public IEnumerable<RoleConfiguration> Roles
        {
            get { return _roles; }
        }

        public ServiceConfiguration(string configDataPath)
            : this(XElement.Parse(configDataPath))
        {}

        public ServiceConfiguration(XElement configData)
        {
            _existingConfiguration = new XElement(configData);
            ServiceName = (string)_existingConfiguration.Attribute("serviceName");
            OsFamily = (int?)_existingConfiguration.Attribute("osFamily") ?? 0;
            OsVersion = (string)_existingConfiguration.Attribute("osVersion");

            _roles = new List<RoleConfiguration>(
                _existingConfiguration.Elements(XmlNamespaces.ServiceConfig + "Role")
                    .Select(element => new RoleConfiguration(element)))
               .AsReadOnly();
        }

        public XElement ToXml()
        {
            _existingConfiguration.SetAttributeValue("serviceName", ServiceName);
            _existingConfiguration.SetAttributeValue("osFamily", OsFamily.ToString(CultureInfo.InvariantCulture));
            _existingConfiguration.SetAttributeValue("osVersion", OsVersion);

            return _existingConfiguration;
        }
    }

    public class RoleConfiguration
    {
        private readonly XElement _roleConfiguration;

        internal RoleConfiguration(XElement roleConfiguration)
        {
            _roleConfiguration = roleConfiguration;
            ConfigurationSettings = new RoleConfigurationSettings(_roleConfiguration.Element(XmlNamespaces.ServiceConfig + "ConfigurationSettings"));
            Certificates = new RoleCertificateConfigurations(_roleConfiguration.Element(XmlNamespaces.ServiceConfig + "Certificates"));
        }

        public string RoleName
        {
            get { return (string)_roleConfiguration.Attribute("name"); }
            set { _roleConfiguration.SetAttributeValue("name", value); }
        }

        public int InstanceCount
        {
            get { return (int)_roleConfiguration.Elements(XmlNamespaces.ServiceConfig + "Instances").Single().Attribute("count"); }
            set { _roleConfiguration.Elements(XmlNamespaces.ServiceConfig + "Instances").Single().SetAttributeValue("count", value); }
        }

        public RoleConfigurationSettings ConfigurationSettings { get; private set; }
        public RoleCertificateConfigurations Certificates { get; private set; }
    }

    public class RoleConfigurationSettings : IEnumerable<RoleConfigurationSetting>
    {
        private readonly XElement _settingsElement;

        public RoleConfigurationSettings(XElement settingsElement)
        {
            _settingsElement = settingsElement;
        }

        public string this[string key]
        {
            get
            {
                var element = GetSettingElement(key);

                return element.Attribute("value").Value;
            }
            set
            {
                var element = GetSettingElement(key);

                element.SetAttributeValue("value", value);
            }
        }

        private XElement GetSettingElement(string key)
        {
            var element = _settingsElement.Elements()
                .FirstOrDefault(e => e.Attribute("name").Value == key);

            if (element == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Role does not have setting with key '{0}'",
                        key));
            }
            return element;
        }

        public IEnumerator<RoleConfigurationSetting> GetEnumerator()
        {
            return _settingsElement.Elements()
                .Select(e => new RoleConfigurationSetting(e))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RoleConfigurationSetting
    {
        private readonly XElement _settingElement;

        public RoleConfigurationSetting(XElement settingElement)
        {
            _settingElement = settingElement;
        }

        public string Name
        {
            get { return _settingElement.Attribute("name").Value; }
        }

        public string Value
        {
            get { return _settingElement.Attribute("value").Value; }
            set { _settingElement.SetAttributeValue("value", value); }
        }
    }

    public class RoleCertificateConfigurations : IEnumerable<CertificateConfiguration>
    {
        private readonly XElement _certificatesConfigurationElement;

        public RoleCertificateConfigurations(XElement certificatesConfigurationElement)
        {
            _certificatesConfigurationElement = certificatesConfigurationElement;
        }

        public CertificateConfiguration this[string key]
        {
            get
            {
                var element = CertificatesConfigurationElements
                    .FirstOrDefault(e => e.Attribute("name").Value == key);

                if (element == null)
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Role does not have a certificate with key '{0}'",
                            key));
                }

                return new CertificateConfiguration(element);
            }
        }

        private IEnumerable<XElement> CertificatesConfigurationElements
        {
            get
            {
                if (_certificatesConfigurationElement == null)
                {
                    return Enumerable.Empty<XElement>();
                }
                return _certificatesConfigurationElement.Elements();
            }
        }

        public IEnumerator<CertificateConfiguration> GetEnumerator()
        {
            return CertificatesConfigurationElements
                .Select(e => new CertificateConfiguration(e))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CertificateConfiguration
    {
        private readonly XElement _certificateConfigurationElement;

        internal CertificateConfiguration(XElement certificateConfigurationElement)
        {
            _certificateConfigurationElement = certificateConfigurationElement;
        }

        public string Name
        {
            get { return (string)_certificateConfigurationElement.Attribute("name"); }
        }

        public string Thumbprint
        {
            get { return (string)_certificateConfigurationElement.Attribute("thumbprint"); }
            set { _certificateConfigurationElement.SetAttributeValue("thumbprint", value); }
        }

        public string ThumbprintAlgorithm
        {
            get { return (string)_certificateConfigurationElement.Attribute("thumbprintAlgorithm"); }
            set { _certificateConfigurationElement.SetAttributeValue("thumbprintAlgorithm", value); }
        }
    }
}
