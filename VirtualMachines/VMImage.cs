using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class VMImage
    {

        internal VMImage(XElement xml, Subscription subscription)
        {
            Subscription = subscription;
            DataDiskConfigurations = new List<DataDiskConfiguration>();
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
        }

        internal VMImage() { }

        internal VMImage(string name, string label, OSDiskConfiguration osDiskConfiguration, string description = null, IEnumerable<DataDiskConfiguration> dataDisks = null,
                         string recommendedVmSize = null, string imageFamily = "", string language = "", string eula = "", string iconUri = "",
                         string smallIconUri = "", string privacyUri = "")
        {
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(!String.IsNullOrEmpty(label));
            Contract.Requires(osDiskConfiguration != null);

            Name = name;
            Label = label;
            OSDiskConfiguration = osDiskConfiguration;
            Description = description;

            DataDiskConfigurations = new List<DataDiskConfiguration>();

            if (dataDisks != null && dataDisks.Any())
                DataDiskConfigurations.AddRange(dataDisks);

            RecommendedVMSize = recommendedVmSize;
            ImageFamily = imageFamily;
            Language = language;
            Eula = eula;
            IconUri = iconUri;
            SmallIconUri = smallIconUri;
            PrivacyUri = privacyUri;
        }

        public async Task CreateVmImageAsync(Subscription subscription)
        {
            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "VMImage",
                new XElement(ns + "Name", Name),
                new XElement(ns + "Label", Label))
                ;

            if (!String.IsNullOrEmpty(Description))
                content.Add(new XElement(ns + "Description", Description));

            var osConfiguration = new XElement(ns + "OSDiskConfiguration");

            osConfiguration.Add(new XElement(ns + "HostCaching", OSDiskConfiguration.HostCaching));
            osConfiguration.Add(new XElement(ns + "OSState", OSDiskConfiguration.OSState.ToString()));
            osConfiguration.Add(new XElement(ns + "OS", OSDiskConfiguration.OS.ToString()));
            osConfiguration.Add(new XElement(ns + "MediaLink", OSDiskConfiguration.MediaLink));
            content.Add(osConfiguration);

            if (DataDiskConfigurations.Any())
            {

                var diskConfigurations = new XElement(ns + "DataDiskConfigurations");

                DataDiskConfigurations.ForEach(x =>
                {
                    var diskConfiguration = new XElement(ns + "DataDiskConfiguration");
                    diskConfiguration.Add(new XElement(ns + "HostCaching", x.HostCaching.ToString()));
                    diskConfiguration.Add(new XElement(ns + "Lun", x.Lun.ToString()));
                    diskConfiguration.Add(new XElement(ns + "MediaLink", x.MediaLink));
                    diskConfiguration.Add(new XElement(ns + "LogicalSizeInGB", x.LogicalDiskSizeInGB));
                    diskConfigurations.Add(diskConfiguration);
                });

                content.Add(diskConfigurations);
            }

            if (!String.IsNullOrEmpty(Language))
                content.Add(new XElement(ns + "Language", Language));

            if (!String.IsNullOrEmpty(ImageFamily))
                content.Add(new XElement(ns + "ImageFamily", ImageFamily));

            if (!String.IsNullOrEmpty(RecommendedVMSize))
                content.Add(new XElement(ns + "RecommendedVMSize", RecommendedVMSize));

            if (!String.IsNullOrEmpty(Eula))
                content.Add(new XElement(ns + "Eula", Eula));

            if (!String.IsNullOrEmpty(IconUri))
                content.Add(new XElement(ns + "IconUri", IconUri));

            if (!String.IsNullOrEmpty(SmallIconUri))
                content.Add(new XElement(ns + "SmallIconUri", SmallIconUri));

            if (!String.IsNullOrEmpty(PrivacyUri))
                content.Add(new XElement(ns + "PrivacyUri", PrivacyUri));


            var client = GetRestClient();
            var response = await client.PostAsync(content);
            await Subscription.WaitForOperationCompletionAsync(response);
        }

        public async Task DeleteOsImageAsync(bool deleteAssociatedBlob)
        {
            var client = GetRestClient(deleteAssociatedBlob ? "?comp=media" : null);
            var response = await client.DeleteAsync();
            await Subscription.WaitForOperationCompletionAsync(response);
        }

        private AzureRestClient GetRestClient(string queryString = null)
        {
            var servicePath = "services/vmimages/" + Name;

            if (!String.IsNullOrEmpty(queryString))
                servicePath += queryString;

            return Subscription.GetDatabaseRestClient(servicePath);
        }

        [Ignore]
        public Subscription Subscription { get; private set; }
        public string Name { get; private set; }
        public string Label { get; private set; }
        public string Category { get; private set; }
        public string Description { get; private set; }

        [Traverse]
        public OSDiskConfiguration OSDiskConfiguration { get; set; }

        [Traverse]
        public List<DataDiskConfiguration> DataDiskConfigurations { get; set; }

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
        public string SmalIconUri { get; private set; }
        public string PrivacyUri { get; private set; }
        public DateTimeOffset? PublishDate { get; private set; }
        public string SmallIconUri { get; private set; }

    }
}