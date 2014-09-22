using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml.Linq;

namespace Linq2Azure.Locations
{
    public class Location
    {
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public IEnumerable<string> AvailableServices { get; private set; }
        public IEnumerable<string> WebWorkerRoleSizes { get; private set; }
        public IEnumerable<string> VirtualMachinesRoleSizes { get; private set; }

        public Subscription Subscription { get; private set; }

        internal Location(XElement xml, Subscription subscription)
        {
            Contract.Requires(Subscription == null);
            Contract.Requires(subscription != null);

            var azureNamespace = XmlNamespaces.WindowsAzure;

            xml.HydrateObject(azureNamespace, this);
            Subscription = subscription;

            var computeCapabilitiesElement = xml.Element(azureNamespace + "ComputeCapabilities");

            AvailableServices = GetAvailableServices(xml, azureNamespace);
            WebWorkerRoleSizes = GetRoleSizes(computeCapabilitiesElement, azureNamespace, "WebWorkerRoleSizes");
            VirtualMachinesRoleSizes = GetRoleSizes(computeCapabilitiesElement, azureNamespace, "VirtualMachinesRoleSizes");
        }

        private static IEnumerable<string> GetAvailableServices(XContainer xml, XNamespace azureNamespace)
        {
            var availableServicesElement = xml.Element(azureNamespace + "AvailableServices");
            if (availableServicesElement == null)
            {
                return Enumerable.Empty<string>();
            }

            return availableServicesElement.Elements(azureNamespace + "AvailableService")
                .Select(e => e.Value)
                .ToList();
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