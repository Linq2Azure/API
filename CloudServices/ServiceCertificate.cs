using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.CloudServices
{
    public class ServiceCertificate
    {
        public Uri Uri { get; private set; }
        public string Thumbprint { get; private set; }
        public string ThumbprintAlgorithm { get; private set; }
        public X509Certificate2 Certificate { get; private set; }
        public CloudService Parent { get; private set; }

        public ServiceCertificate(X509Certificate2 certificate)
        {
            Contract.Requires(certificate != null);
            Certificate = certificate;
            Thumbprint = certificate.Thumbprint;
            ThumbprintAlgorithm = certificate.SignatureAlgorithm.FriendlyName == "sha1RSA" ? "sha1" : certificate.SignatureAlgorithm.FriendlyName;
        }

        public ServiceCertificate(XElement element, CloudService parent)
        {
            var ns = XmlNamespaces.WindowsAzure;
            Uri = new Uri((string)element.Element(ns + "CertificateUrl"));
            Thumbprint = (string)element.Element(ns + "Thumbprint");
            ThumbprintAlgorithm = (string)element.Element(ns + "ThumbprintAlgorithm");
            Certificate = new X509Certificate2(Convert.FromBase64String((string)element.Element(ns + "Data")));
            Parent = parent;
        }

        public async Task AddAsync(CloudService parent)
        {
            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "CertificateFile",
                new XElement(ns + "Data", Convert.ToBase64String(Certificate.Export(X509ContentType.Pfx))),
                //new XElement(ns + "Data", Convert.ToBase64String(File.ReadAllBytes (@"c:\temp\centralserver.sprint.cer"))),
                new XElement(ns + "CertificateFormat", "pfx")
                ,new XElement(ns + "Password")
                );

            HttpResponseMessage response = await GetRestClient(parent).PostAsync(content);
            await parent.Subscription.WaitForOperationCompletionAsync(response);
            Parent = parent;
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Parent != null);
            await Parent.Subscription.WaitForOperationCompletionAsync(await GetRestClient("/" + ThumbprintAlgorithm + "-" + Thumbprint).DeleteAsync());
            Parent = null;
        }

        AzureRestClient GetRestClient(string suffix = null) { return GetRestClient(Parent, suffix); }

        AzureRestClient GetRestClient(CloudService parent, string pathSuffix = null)
        {
            string servicePath = "services/hostedServices/" + parent.Name + "/certificates";
            if (pathSuffix != null) servicePath += pathSuffix;
            return parent.Subscription.GetCoreRestClient(servicePath);
        }

    }
}
