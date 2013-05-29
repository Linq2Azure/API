using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure
{
    public class ServiceConfiguration
    {
        public string ServiceName { get; set; }
        public int OsFamily { get; set; }
        public string OsVersion { get; set; }
        public List<RoleConfiguration> ConfigurationItems { get; set; }

        public ServiceConfiguration()
        {
            ConfigurationItems = new List<RoleConfiguration>();
            OsVersion = "*";
        }

        public ServiceConfiguration(XElement configData)
        {
            ServiceName = (string)configData.Attribute("serviceName");
            OsFamily = (int?)configData.Attribute("osFamily") ?? 0;
            OsVersion = (string)configData.Attribute("osVersion");

            ConfigurationItems = new List<RoleConfiguration>(
                configData.Elements(XmlNamespaces.ServiceConfig + "Role").Select(RoleConfiguration.Load));
        }

        public XElement ToXml()
        {
            return new XElement(XmlNamespaces.ServiceConfig + "ServiceConfiguration",
                new XAttribute("serviceName", ServiceName),
                new XAttribute("osFamily", OsFamily),
                new XAttribute("osVersion", OsVersion),
                new XAttribute("schemaVersion", "2012-05.1.7"),
                ConfigurationItems.Select(c => c.ToXml())
                );
        }
    }

    public class RoleConfiguration
    {
        public XElement ToXml()
        {
            return new XElement(XmlNamespaces.ServiceConfig + "Role", new XAttribute("name", RoleName),
                new XElement(XmlNamespaces.ServiceConfig + "Instances", new XAttribute("count", InstanceCount)),
                new XElement(XmlNamespaces.ServiceConfig + "ConfigurationSettings",
                    ConfigurationSettings.Select(kvp => new XElement(XmlNamespaces.ServiceConfig + "Setting", new XAttribute("name", kvp.Key), new XAttribute("value", kvp.Value)))),
                Certificates == null || Certificates.Count == 0 ? null : new XElement(XmlNamespaces.ServiceConfig + "Certificates", Certificates.Values.Select(c => c.ToXml()))
                );
        }

        public static RoleConfiguration Load(XElement element)
        {
            var rc = new RoleConfiguration();
            rc.RoleName = (string)element.Attribute("name");
            rc.InstanceCount = (int)element.Elements(XmlNamespaces.ServiceConfig + "Instances").Single().Attribute("count");
            rc.ConfigurationSettings = element.Elements(XmlNamespaces.ServiceConfig + "ConfigurationSettings").SelectMany(d => d.Elements()).ToDictionary(x => (string)x.Attribute("name"), x => (string)x.Attribute("value"));
            rc.Certificates = element.Elements(XmlNamespaces.ServiceConfig + "Certificates").SelectMany(d => d.Elements()).ToDictionary(x => (string)x.Attribute("name"), x => new CertificateConfig(x));
            return rc;
        }

        public string RoleName { get; set; }
        public int InstanceCount { get; set; }
        public IDictionary<string, string> ConfigurationSettings { get; set; }
        public IDictionary<string, CertificateConfig> Certificates { get; set; }
    }

    public class CertificateConfig
    {
        public string Name { get; set; }
        public string Thumbprint { get; set; }
        public string ThumbprintAlgorithm { get; set; }

        public XElement ToXml()
        {
            return new XElement("Certificate",
                new XAttribute("name", Name),
                new XAttribute("thumbprint", Thumbprint),
                new XAttribute("thumbprintAlgorithm", ThumbprintAlgorithm));
        }

        public CertificateConfig(XElement xml)
        {
            Name = (string)xml.Attribute("name");
            Thumbprint = (string)xml.Attribute("thumbprint");
            ThumbprintAlgorithm = (string)xml.Attribute("thumbprintAlgorithm");
        }
    }
}
