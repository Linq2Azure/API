using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class OSImage
    {
        internal OSImage(XElement xml, Subscription subscription)
        {
            Subscription = subscription;
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
            Debug.WriteLine(xml);
        }
        public Subscription Subscription { get; private set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string OS { get; set; }
        public string MediaLink { get; set; }
        public string ServiceName { get; set; }
        public string DeploymentName { get; set; }
        public string RoleName { get; set; }
        public string Location { get; set; }
        public string AffinityGroup { get; set; }
        public DateTimeOffset? CreateTime { get; set; }
        public DateTimeOffset? ModifiedTime { get; set; }
        public string Language { get; set; }
        public string ImageFamily { get; set; }
        public string RecommendedVMSize { get; set; }
        public bool IsPremium { get; set; }
        public string Eula { get; set; }
        public string IconUri { get; set; }
        public string SmalIconUri { get; set; }
        public string PrivacyUri { get; set; }
        public DateTimeOffset? PublishDate { get; set; }
    }
}