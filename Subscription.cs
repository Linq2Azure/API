using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using System.Threading;

namespace Linq2Azure
{
    public class Subscription
    {
        public static Subscription FromPublisherSettingsPath(string publishingSettingsPath)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(publishingSettingsPath));

            return (
                from c in XDocument.Load(publishingSettingsPath).Descendants("PublishProfile")
                from s in c.Descendants("Subscription")
                select new Subscription
                {
                    SubscriptionID = Guid.Parse((string)s.Attribute("Id")),
                    SubscriptionName = (string)s.Attribute("Name"),
                    ManagementCertificate =
                        new X509Certificate2(
                        Convert.FromBase64String((string)c.Attribute("ManagementCertificate"))),
                }).Single();
        }

        public X509Certificate2 ManagementCertificate { get; set; }
        public Guid SubscriptionID { get; set; }
        public string SubscriptionName { get; set; }

        public IObservable<CloudService> CloudServices
        {
            get { return GetCloudServices().ToObservable().SelectMany(x => x); }
        }

        internal AzureRestClient GetRestClient(string relativeUri)
        {
            return new AzureRestClient(this, relativeUri);
        }

        async Task<CloudService[]> GetCloudServices()
        {
            XElement xe = await GetRestClient("hostedServices").GetXmlAsync();
            return xe.Descendants(XmlNamespaces.Base + "HostedService").Select(x => new CloudService(x, this)).ToArray();
        }
    }
}
