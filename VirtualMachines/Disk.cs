using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class Disk
    {

        internal Disk(XElement xml, Subscription subscription)
        {
            Contract.Requires(subscription != null);
            Contract.Requires(xml != null);

            Subscription = subscription;
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
        }

        internal Disk() { }

        internal Disk(OsType os, string name, string mediaLink)
        {

            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(!String.IsNullOrEmpty(mediaLink));

            OS = os;
            Name = name;
            MediaLink = mediaLink;
        }

        public async Task CreateDiskAsync(Subscription subscription, string label)
        {
            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "Disk",
                new XElement(ns + "OS", OS),
                new XElement(ns + "Label", label),
                new XElement(ns + "MediaLink", MediaLink),
                new XElement(ns + "Name", Name))
                ;

            var client = GetRestClient();
            var response = await client.PostAsync(content);
            await Subscription.WaitForOperationCompletionAsync(response);
        }

        public async Task<Disk> ResizeDiskAsync(int resizedSizeInGB)
        {
            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "Disk",
                new XElement(ns + "Name", Name),
                new XElement(ns + "ResizedSizeInGB", resizedSizeInGB));

            var client = GetRestClient();
            var response = await client.PutAsync(content);
            await Subscription.WaitForOperationCompletionAsync(response);
            return this;
        }

        public async Task<Disk> ReLabeleDiskAsync(string label)
        {
            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "Disk",
                new XElement(ns + "Name", Name),
                new XElement(ns + "Label", label));

            var client = GetRestClient();
            var response = await client.PutAsync(content);
            await Subscription.WaitForOperationCompletionAsync(response);
            return this;
        }

        public async Task<Disk> DeleteDiskAsync(bool deleteAssociatedBlob)
        {
            var client = GetRestClient(deleteAssociatedBlob ? "?comp=media" : null);
            var response = await client.DeleteAsync();
            await Subscription.WaitForOperationCompletionAsync(response);
            return this;
        }

        private AzureRestClient GetRestClient(string queryString = null)
        {
            var servicePath = "services/disks/" + Name;

            if (!String.IsNullOrEmpty(queryString))
                servicePath += queryString;

            return Subscription.GetDatabaseRestClient(servicePath);
        }

        public Subscription Subscription { get; private set; }
        public string AffinityGroup { get; private set; }

        [Traverse]
        public AttachedTo AttachedTo { get; private set; }
        public OsType OS { get; private set; }
        public string Location { get; private set; }
        public string LogicalSizeInGB { get; private set; }
        public string MediaLink { get; private set; }
        public string Name { get; private set; }
        public string SourceImageName { get; private set; }
        public DateTimeOffset CreatedTime { get; private set; }
        public string IOType { get; private set; }
    }
}