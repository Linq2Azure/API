using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.StorageAccounts
{
    public enum StorageAccountGeoReplication
    {
        Disabled = 0,
        Enabled = 1,
        ReadAccessEnabled = 2
    }
    public class StorageAccount
    {
        public StorageAccount(
            string serviceName,
            string description,
            IDeploymentAssociation deploymentAssociation,
            StorageAccountGeoReplication geoReplication)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(serviceName));
            Contract.Requires(!string.IsNullOrWhiteSpace(description));
            Contract.Requires(deploymentAssociation != null);

            ServiceName = Label = serviceName;
            Description = description;
            deploymentAssociation.AssignValue(location => Location = location, affinityGroup => AffinityGroup = affinityGroup);
            GeoReplicationEnabled = geoReplication != StorageAccountGeoReplication.Disabled;
            SecondaryReadEnabled = geoReplication == StorageAccountGeoReplication.ReadAccessEnabled;
            ExtendedProperties = new Dictionary<string, string>();
        }

        internal StorageAccount(XElement xml, Subscription subscription)
        {
            var azureNamespace = XmlNamespaces.WindowsAzure;

            xml.HydrateObject(azureNamespace, this);
            Subscription = subscription;

            var storageServicePropertiesElement = xml.Element(azureNamespace + "StorageServiceProperties");
            storageServicePropertiesElement.HydrateObject(azureNamespace, this);

            if (!string.IsNullOrEmpty(Label)) Label = Label.FromBase64String();

            var extendedProperties = xml.Element(azureNamespace + "ExtendedProperties");
            if (extendedProperties != null)
            {
                ExtendedProperties = extendedProperties.Elements()
                    .ToDictionary(
                        x => (string) x.Element(azureNamespace + "Name"),
                        x => (string) x.Element(azureNamespace + "Value"));
            }
        }

        public Subscription Subscription { get; private set; }
        public Uri Url { get; private set; }
        public string ServiceName { get; private set; }
        public string Description { get; private set; }
        public string Label { get; private set; }
        public string AffinityGroup { get; private set; }
        public string Location { get; private set; }
        public bool GeoReplicationEnabled { get; private set; }
        public bool SecondaryReadEnabled { get; private set; }
        public IDictionary<string, string> ExtendedProperties { get; set; }

        public StorageAccountGeoReplication StorageAccountGeoReplication
        {
            get
            {
                if (!GeoReplicationEnabled)
                {
                    return StorageAccountGeoReplication.Disabled;
                }

                return SecondaryReadEnabled
                    ? StorageAccountGeoReplication.ReadAccessEnabled
                    : StorageAccountGeoReplication.Enabled;
            }
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Subscription != null);

            await GetRestClient().DeleteAsync();

            Subscription = null;
        }

        private AzureRestClient GetRestClient(string pathSuffix = null)
        {
            if (Subscription == null)
            {
                throw new InvalidOperationException("Subscription cannot be null for this operation.");
            }
            var servicePath = "services/storageservices/" + ServiceName;
            if (!string.IsNullOrEmpty(pathSuffix))
            {
                servicePath += pathSuffix;
            }
            return Subscription.GetCoreRestClient20120301(servicePath);
        }

        internal async Task CreateAsync(Subscription subscription)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(ServiceName));
            Contract.Requires(!string.IsNullOrWhiteSpace(Label));
            Contract.Requires(Location == null || Location.Trim().Length > 0);
            Contract.Requires(AffinityGroup == null || AffinityGroup.Trim().Length > 0);
            Contract.Requires((Location == null) != (AffinityGroup == null));

            var azureNamespace = XmlNamespaces.WindowsAzure;

            var content = new XElement(azureNamespace + "CreateStorageServiceInput",
                new XElement(azureNamespace + "ServiceName", ServiceName),
                string.IsNullOrWhiteSpace(Description) ? null : new XElement(azureNamespace + "Description", Description),
                new XElement(azureNamespace + "Label", Label.ToBase64String()),
                string.IsNullOrWhiteSpace(Location) ? null : new XElement(azureNamespace + "Location", Location),
                string.IsNullOrWhiteSpace(AffinityGroup) ? null : new XElement(azureNamespace + "AffinityGroup", AffinityGroup),
                new XElement(azureNamespace + "GeoReplicationEnabled", GeoReplicationEnabled ? "true" : "false"),
                ExtendedProperties == null || ExtendedProperties.Count == 0
                    ? new XElement(azureNamespace + "ExtendedProperties")
                    : new XElement(azureNamespace + "ExtendedProperties", ExtendedProperties.Select(kv =>
                        new XElement(azureNamespace + "ExtendedProperty",
                            new XElement(azureNamespace + "Name", kv.Key),
                            new XElement(azureNamespace + "Value", kv.Value)))),
                new XElement(azureNamespace + "SecondaryReadEnabled", SecondaryReadEnabled ? "true" : "false"));

            var hc = subscription.GetCoreRestClient20131101("services/storageservices");
            await hc.PostAsync(content);
            Subscription = subscription;
        }
    }
}