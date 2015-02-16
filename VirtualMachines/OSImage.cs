using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class OSImage
    {
        internal OSImage(XElement xml, Subscription subscription)
        {

            Contract.Requires(subscription != null);

            Subscription = subscription;
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
        }

        internal OSImage(string label, string name, string mediaLink, OsType os, string description = "", string imageFamily = "", 
                         string language = "", string eula = "", string privacyUri = "", string iconUri = "", string smallIconUri = "")
        {

            Contract.Requires(!String.IsNullOrEmpty(label));
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(!String.IsNullOrEmpty(mediaLink));

            Label = label;
            MediaLink = mediaLink;
            Name = name;
            OS = os;
            Description = description;
            ImageFamily = imageFamily;
            Language = language;
            PrivacyUri = privacyUri;
            IconUri = iconUri;
            SmallIconUri = smallIconUri;
            Eula = eula;
        }

        public async Task<OSImage> UpdateOsImageAsync()
        {
            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "OSImage");
            content.Add(new XElement(ns + "Label", Label));

            if (!String.IsNullOrEmpty(Eula))
                content.Add(new XElement(ns + "Eula", Eula));

            if (!String.IsNullOrEmpty(Description))
                content.Add(new XElement(ns + "Description", Description));

            if (!String.IsNullOrEmpty(ImageFamily))
                content.Add(new XElement(ns + "ImageFamily", ImageFamily));

            if (!String.IsNullOrEmpty(PrivacyUri))
                content.Add(new XElement(ns + "PrivacyUri", PrivacyUri));

            if (!String.IsNullOrEmpty(IconUri))
                content.Add(new XElement(ns + "IconUri", IconUri));

            if (!String.IsNullOrEmpty(RecommendedVMSize))
                content.Add(new XElement(ns + "RecommendedVMSize", RecommendedVMSize));

            if (!String.IsNullOrEmpty(SmallIconUri))
                content.Add(new XElement(ns + "SmallIconUri", SmallIconUri));

            if (!String.IsNullOrEmpty(Language))
                content.Add(new XElement(ns + "Language", Language));


            var client = GetRestClient();
            var response = await client.PutAsync(content);
            await Subscription.WaitForOperationCompletionAsync(response);
            return this;
        }

        public async Task CreateOsImageAsync(Subscription subscription)
        {
            var client = GetRestClient();

            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "OSImage");
            content.Add(new XElement(ns + "Label", Label));
            content.Add(new XElement(ns + "MediaLink", MediaLink));
            content.Add(new XElement(ns + "Name", Name));
            content.Add(new XElement(ns + "OS", OS.ToString()));

            if (!String.IsNullOrEmpty(Eula))
                content.Add(new XElement(ns + "Eula", Eula));

            if (!String.IsNullOrEmpty(Description))
                content.Add(new XElement(ns + "Description", Description));

            if (!String.IsNullOrEmpty(ImageFamily))
                content.Add(new XElement(ns + "ImageFamily", ImageFamily));

            if (!String.IsNullOrEmpty(PrivacyUri))
                content.Add(new XElement(ns + "PrivacyUri", PrivacyUri));

            if (!String.IsNullOrEmpty(IconUri))
                content.Add(new XElement(ns + "IconUri", IconUri));

            if (!String.IsNullOrEmpty(RecommendedVMSize))
                content.Add(new XElement(ns + "RecommendedVMSize", RecommendedVMSize));

            if (!String.IsNullOrEmpty(SmallIconUri))
                content.Add(new XElement(ns + "SmallIconUri", SmallIconUri));

            if (!String.IsNullOrEmpty(Language))
                content.Add(new XElement(ns + "Language", Language));

            var response = await client.PostAsync(content);
            await Subscription.WaitForOperationCompletionAsync(response);
        }

        public async Task<OSImage> DeleteOsImageAsync(bool deleteAssociatedBlob)
        {
            var client = GetRestClient(deleteAssociatedBlob ? "?comp=media" : null);
            var response = await client.DeleteAsync();
            await Subscription.WaitForOperationCompletionAsync(response);
            return this;
        }

        private AzureRestClient GetRestClient(string queryString = null)
        {
            var servicePath = "services/images/" + Name;

            if (!String.IsNullOrEmpty(queryString))
                servicePath += queryString;

            return Subscription.GetDatabaseRestClient(servicePath);
        }

        public Subscription Subscription { get; private set; }
        public string Name { get; private set; }
        public string Label { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }
        public OsType OS { get; private set; }
        public string MediaLink { get; private set; }
        public string ServiceName { get; private set; }
        public string DeploymentName { get; private set; }
        public string RoleName { get; private set; }
        public string Location { get; private set; }
        public string AffinityGroup { get; private set; }
        public DateTimeOffset? CreateTime { get; private set; }
        public DateTimeOffset? ModifiedTime { get; private set; }
        public string Language { get; private set; }
        public string ImageFamily { get; private set; }
        public string RecommendedVMSize { get; private set; }
        public bool IsPremium { get; private set; }
        public string Eula { get; private set; }
        public string IconUri { get; private set; }
        public string SmallIconUri { get; private set; }
        public string PrivacyUri { get; private set; }
        public DateTimeOffset? PublishDate { get; private set; }
    }
}