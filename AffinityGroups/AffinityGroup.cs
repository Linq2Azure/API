using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.AffinityGroups
{
    public class AffinityGroup
    {
        public string Name { get; private set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Location { get; private set; }
        public Subscription Subscription { get; private set; }
        public DateTimeOffset? CreatedTime { get; private set; }
        public IEnumerable<string> Capabilities { get; private set; }
        public IEnumerable<string> WebWorkerRoleSizes { get; private set; }
        public IEnumerable<string> VirtualMachinesRoleSizes { get; private set; }

        public AffinityGroup(string name, string location)
            : this(name: name, label: name, description: null, location: location)
        {}

        public AffinityGroup(string name, string label, string description, string location)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(label));
            Contract.Requires(!string.IsNullOrWhiteSpace(description));
            Contract.Requires(!string.IsNullOrWhiteSpace(location));

            Name = name;
            Label = label;
            Description = description;
            Location = location;
            Capabilities = new List<string>();
            WebWorkerRoleSizes = new List<string>();
            VirtualMachinesRoleSizes = new List<string>();
        }

        internal AffinityGroup(XElement xml, Subscription subscription)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);

            var azureNamespace = XmlNamespaces.WindowsAzure;

            xml.HydrateObject(azureNamespace, this);
            Subscription = subscription;

            if (!string.IsNullOrEmpty(Label))
            {
                Label = Label.FromBase64String();
            }

            var computeCapabilitiesElement = xml.Element(azureNamespace + "ComputeCapabilities");

            Capabilities = GetCapabilities(xml, azureNamespace);
            WebWorkerRoleSizes = GetRoleSizes(computeCapabilitiesElement, azureNamespace, "WebWorkerRoleSizes");
            VirtualMachinesRoleSizes = GetRoleSizes(computeCapabilitiesElement, azureNamespace, "VirtualMachinesRoleSizes");
        }

        internal async Task CreateAsync(Subscription subscription)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(Name));
            Contract.Requires(!string.IsNullOrWhiteSpace(Label));
            Contract.Requires(!string.IsNullOrWhiteSpace(Location));

            var azureNamespace = XmlNamespaces.WindowsAzure;

            var content = new XElement(azureNamespace + "CreateAffinityGroup",
                new XElement(azureNamespace + "Name", Name),
                new XElement(azureNamespace + "Label", Label.ToBase64String()),
                string.IsNullOrWhiteSpace(Description) ? null : new XElement(azureNamespace + "Description", Description),
                string.IsNullOrWhiteSpace(Location) ? null : new XElement(azureNamespace + "Location", Location));

            var hc = subscription.GetCoreRestClient20140601("affinitygroups");
            await hc.PostAsync(content);
            Subscription = subscription;
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Subscription != null);
            await GetRestClient("/" + Name).DeleteAsync();
            Subscription = null;
        }

        public async Task UpdateAsync()
        {
            Contract.Requires(Subscription != null);

            var azureNamespace = XmlNamespaces.WindowsAzure;

            var content = new XElement(azureNamespace + "UpdateAffinityGroup",
                new XElement(azureNamespace + "Label", Label.ToBase64String()),
                string.IsNullOrWhiteSpace(Description) ? null : new XElement(azureNamespace + "Description", Description));

            var hc = GetRestClient("/" + Name);
            await hc.PutAsync(content);
        }

        private AzureRestClient GetRestClient(string pathSuffix = null)
        {
            return GetRestClient(Subscription, pathSuffix);
        }

        private static AzureRestClient GetRestClient(Subscription subscription, string pathSuffix = null)
        {
            if (subscription == null)
            {
                throw new InvalidOperationException("Subscription cannot be null for this operation.");
            }
            var servicePath = "affinitygroups";
            if (!string.IsNullOrEmpty(pathSuffix))
            {
                servicePath += pathSuffix;
            }
            return subscription.GetCoreRestClient20140601(servicePath);
        }

        private static IEnumerable<string> GetCapabilities(XContainer xml, XNamespace azureNamespace)
        {
            var capabilitiesElement = xml.Element(azureNamespace + "Capabilities");
            if (capabilitiesElement == null)
            {
                return Enumerable.Empty<string>();
            }

            return capabilitiesElement.Elements(azureNamespace + "Capability")
                .Select(e => e.Value);
        }

        private static IEnumerable<string> GetRoleSizes(
            XContainer computeCapabilitiesElement,
            XNamespace azureNamespace,
            string subElementName)
        {
            var capabilitiesElement = computeCapabilitiesElement.Element(azureNamespace + subElementName);
            if (capabilitiesElement == null)
            {
                return Enumerable.Empty<string>();
            }

            return capabilitiesElement.Elements(azureNamespace + "RoleSize")
                .Select(e => e.Value);
        }
    }
}