using System;
using System.Collections.Generic;
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
        private readonly Subscription _subscription;
        private readonly CloudService _parent;

        private Deployment(Subscription subscription, CloudService parent)
        {
            if (subscription == null) throw new ArgumentNullException("subscription");
            _subscription = subscription;
            _parent = parent;
        }

        internal static Deployment LoadAttached(XElement element, Subscription subscription, CloudService parent)
        {
            var x = new Deployment(subscription, parent);
            x.Hydrate(element);
            return x;
        }

        private void Hydrate(XElement element)
        {
            Name = (string)element.Element("{http://schemas.microsoft.com/windowsazure}Url");
            DeploymentSlot = (string)element.Element("{http://schemas.microsoft.com/windowsazure}DeploymentSlot");
            DeploymentId = (string)element.Element("{http://schemas.microsoft.com/windowsazure}PrivateID");
            Configuration = LoadConfigFile((string)element.Element("{http://schemas.microsoft.com/windowsazure}Configuration"));
            ConfigurationFile = DecodingHelper.GetString((string)element.Element("{http://schemas.microsoft.com/windowsazure}Configuration"));
        }

        private static RoleConfiguration[] LoadConfigFile(string base64EncodedConfigFile)
        {
            var doc = XDocument.Parse(DecodingHelper.GetString(base64EncodedConfigFile));
            return doc.Descendants("{http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration}Role")
                .Select(RoleConfiguration.Load)
                .ToArray();
        }

        //public IObservable<IDeploymentLogFile> LogFiles
        //{
        //    get { return this.GetDeploymentLogFiles(); }
        //}

        public string Name { get; set; }
        public string DeploymentSlot { get; set; }
        public string DeploymentId { get; set; }
        public IEnumerable<RoleConfiguration> Configuration { get; set; }
        public string ConfigurationFile { get; set; }

        public string UpdateDeploymentConfiguration()
        {
            if (_subscription == null) throw new InvalidOperationException();
            if (_parent == null) throw new InvalidOperationException();

            // URI variable.
            Uri requestUri = null;

            const string changeConfig = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <ChangeConfiguration xmlns=""http://schemas.microsoft.com/windowsazure"">
                    <Configuration>{0}</Configuration>
                </ChangeConfiguration>";

            string configData = Convert.ToBase64String(Encoding.ASCII.GetBytes(ConfigurationFile));

            string requestBody = string.Format(changeConfig, configData);

            // Create the request.
            requestUri = new Uri("https://management.core.windows.net/"
                                 + _subscription.SubscriptionId
                                 + "/services/"
                                 + "hostedservices/"
                                 + _parent.ServiceName + "/"
                                 + "deploymentslots/"
                                 + DeploymentSlot.ToLower() + "/"
                                 + "?comp=config");

            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(requestUri);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/xml";
            httpWebRequest.ContentLength = Encoding.UTF8.GetBytes(requestBody).Length;

            // Add the certificate to the request.
            httpWebRequest.ClientCertificates.Add(_subscription.ManagementCertificate);

            // Specify the version information in the header.
            httpWebRequest.Headers.Add("x-ms-version", "2012-03-01");

            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                sw.Write(requestBody);
                sw.Close();
            }

            // Make the call using the web request.
            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            // TODO: handle other status codes?
            // Display the web response status code.
            //Console.WriteLine("Response status code: " + httpWebResponse.StatusCode);

            string xmlResponse;

            // Parse the web response.
            using (var responseStream = httpWebResponse.GetResponseStream())
            {
                using (var reader = new StreamReader(responseStream))
                {
                    xmlResponse = reader.ReadToEnd();
                    reader.Close();
                }
                // Close the resources no longer needed.
                httpWebResponse.Close();
            }

            return xmlResponse;

            //var xdocument = XDocument.Parse(xmlResponse);
            //return xdocument.Descendants(@"{http://schemas.microsoft.com/windowsazure}Deployment").Select(x => Deployment.LoadAttached(x, _subscription, this)).ToArray();
        }
    }

    public class RoleConfiguration
    {
        public static RoleConfiguration Load(XElement element)
        {
            var x = new RoleConfiguration();
            x.Hydrate(element);
            return x;
        }

        private void Hydrate(XElement element)
        {
            RoleName = (string)element.Attribute("name");
            InstanceCount = (int)element.Descendants("{http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration}Instances").Single().Attribute("count");
            ConfigurationSettings = element.Descendants("{http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration}ConfigurationSettings").SelectMany(d => d.Elements()).ToDictionary(x => (string)x.Attribute("name"), x => (string)x.Attribute("value"));
            Certificates = element.Descendants("{http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration}Certificates").SelectMany(d => d.Elements()).ToDictionary(x => (string)x.Attribute("name"), x =>
                new CertificateConfig
                    {
                        Name = (string)x.Attribute("name"),
                        Thumbprint = (string)x.Attribute("thumbprint"),
                        ThumbprintAlgorithm = (string)x.Attribute("thumbprintAlgorithm"),
                    });
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
    }

    static class DecodingHelper
    {
        public static string GetString(string base64encodedConfigurationFile)
        {
            return System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(base64encodedConfigurationFile));
        }
    }

}