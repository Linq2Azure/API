using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure
{
    public class Deployment
    {
        internal static Deployment Load(XElement element, CloudService parent)
        {
            Contract.Requires(element != null);
            Contract.Requires(parent != null);

            var x = new Deployment { Parent = parent };
            x.DeploymentName = (string)element.Element(Constants.AzureXmlNamespace + "Name");
            x.Url = (string)element.Element(Constants.AzureXmlNamespace + "Url");
            x.DeploymentSlot = (string)element.Element(Constants.AzureXmlNamespace + "DeploymentSlot");
            x.DeploymentId = (string)element.Element(Constants.AzureXmlNamespace + "PrivateID");
            x.Label = ((string)element.Element(Constants.AzureXmlNamespace + "Label")).FromBase64String();
            x.Configuration = new ServiceConfiguration(element.Element(Constants.AzureXmlNamespace + "Configuration"));
            return x;
        }

        public CloudService Parent { get; private set; }

        Subscription Subscription { get { return Parent.Subscription; } }

        public Deployment()
        {
            Configuration = new ServiceConfiguration();
        }

        public string DeploymentName { get; set; }
        public string Url { get; set; }
        public string DeploymentSlot { get; set; }
        public string DeploymentId { get; set; }
        public string Label { get; set; }
        public ServiceConfiguration Configuration { get; set; }

//        public string UpdateDeploymentConfiguration()
//        {
//            Uri requestUri = null;

//            const string changeConfig = @"<?xml version=""1.0"" encoding=""utf-8""?>
//                <ChangeConfiguration xmlns=""http://schemas.microsoft.com/windowsazure"">
//                    <Configuration>{0}</Configuration>
//                </ChangeConfiguration>";

//            string configData = Convert.ToBase64String(Encoding.ASCII.GetBytes(ConfigurationXml));

//            string requestBody = string.Format(changeConfig, configData);

//            // Create the request.
//            requestUri = new Uri("https://management.core.windows.net/"
//                                 + Subscription.SubscriptionId
//                                 + "/services/"
//                                 + "hostedservices/"
//                                 + Parent.ServiceName + "/"
//                                 + "deploymentslots/"
//                                 + DeploymentSlot.ToLower() + "/"
//                                 + "?comp=config");

//            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(requestUri);
//            httpWebRequest.Method = "POST";
//            httpWebRequest.ContentType = "application/xml";
//            httpWebRequest.ContentLength = Encoding.UTF8.GetBytes(requestBody).Length;

//            // Add the certificate to the request.
//            httpWebRequest.ClientCertificates.Add(Subscription.ManagementCertificate);

//            // Specify the version information in the header.
//            httpWebRequest.Headers.Add("x-ms-version", "2012-03-01");

//            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
//            {
//                sw.Write(requestBody);
//                sw.Close();
//            }

//            // Make the call using the web request.
//            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

//            // TODO: handle other status codes?
//            // Display the web response status code.
//            //Console.WriteLine("Response status code: " + httpWebResponse.StatusCode);

//            string xmlResponse;

//            // Parse the web response.
//            using (var responseStream = httpWebResponse.GetResponseStream())
//            {
//                using (var reader = new StreamReader(responseStream))
//                {
//                    xmlResponse = reader.ReadToEnd();
//                    reader.Close();
//                }
//                // Close the resources no longer needed.
//                httpWebResponse.Close();
//            }

//            return xmlResponse;

//            //var xdocument = XDocument.Parse(xmlResponse);
//            //return xdocument.Descendants(@Constants.AzureXmlNamespace + "Deployment").Select(x => Deployment.LoadAttached(x, _subscription, this)).ToArray();
//        }
    }

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
            OsFamily = (int)configData.Attribute("osFamily");
            OsVersion = (string)configData.Attribute("osVersion");

            ConfigurationItems = new List<RoleConfiguration>(
                configData.Descendants(Constants.AzureServiceConfigXmlNamespace + "Role")
                             .Select(RoleConfiguration.Load));
        }

        public XElement ToXml()
        {
            return new XElement(Constants.AzureServiceConfigXmlNamespace + "ServiceConfiguration",
                new XAttribute ("serviceName", ServiceName),
                new XAttribute ("osFamily", OsFamily),
                new XAttribute ("osVersion", OsVersion),
                new XAttribute ("schemaVersion", "2012-05.1.7"),
                ConfigurationItems.Select(c => c.ToXml())
                );
        }
    }

    public class RoleConfiguration
    {
        public XElement ToXml()
        {
            return new XElement(Constants.AzureServiceConfigXmlNamespace + "Role", new XAttribute("name", RoleName),
                new XElement(Constants.AzureServiceConfigXmlNamespace + "Instances", new XAttribute("count", InstanceCount)),
                new XElement(Constants.AzureServiceConfigXmlNamespace + "ConfigurationSettings",
                    ConfigurationSettings.Select(kvp => new XElement(Constants.AzureServiceConfigXmlNamespace + "Setting", new XAttribute("name", kvp.Key), new XAttribute("value", kvp.Value)))),
                Certificates == null || Certificates.Count == 0 ? null : new XElement(Constants.AzureServiceConfigXmlNamespace + "Certificates", Certificates.Values.Select(c => c.ToXml()))
                );
        }

        public static RoleConfiguration Load(XElement element)
        {
            var rc = new RoleConfiguration();
            rc.RoleName = (string)element.Attribute("name");
            rc.InstanceCount = (int)element.Descendants(Constants.AzureServiceConfigXmlNamespace + "Instances").Single().Attribute("count");
            rc.ConfigurationSettings = element.Descendants(Constants.AzureServiceConfigXmlNamespace + "ConfigurationSettings").SelectMany(d => d.Elements()).ToDictionary(x => (string)x.Attribute("name"), x => (string)x.Attribute("value"));
            rc.Certificates = element.Descendants(Constants.AzureServiceConfigXmlNamespace + "Certificates").SelectMany(d => d.Elements()).ToDictionary(x => (string)x.Attribute("name"), x => new CertificateConfig(x));
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