using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class DataVirtualHardDisk
    {

        public DataVirtualHardDisk()
        {
            HostCaching = HostCaching.None;
        }

        internal DataVirtualHardDisk(Role role) : this()
        {
            Contract.Requires(role != null);
            Role = role;
        }

        internal DataVirtualHardDisk(Role role, string diskLabel) : this(role)
        {
            Contract.Requires(!String.IsNullOrEmpty(diskLabel));
            DiskLabel = diskLabel;
        }

        public DataVirtualHardDisk(HostCaching caching, string diskLabel, string diskName, int lun, int sizeInGb, string mediaLink)
            : this(caching, diskLabel, diskName, lun, mediaLink)
        {
            Contract.Requires(sizeInGb > 0);
            LogicalDiskSizeInGB = sizeInGb;
        }

        public DataVirtualHardDisk(HostCaching caching, string diskLabel, string diskName, int lun,  string mediaLink)
        {

            Contract.Requires(!String.IsNullOrEmpty(diskLabel));
            Contract.Requires(!String.IsNullOrEmpty(diskName));
            Contract.Requires(!String.IsNullOrEmpty(mediaLink));
            Contract.Requires(lun >= 0 && lun <= 31);

            HostCaching = caching;
            DiskLabel = diskLabel;
            DiskName = diskName;
            MediaLink = mediaLink;
            Lun = lun;
        }

        internal void AssignRole(Role role)
        {
            Contract.Requires(role != null);
            Role = role;
        }

        internal async Task AddEmptyDataDiskAsync(Role role)
        {

            Contract.Requires(role != null);
            Role = role;

            var suffix = Role.Deployment.Name + "/roles/" + Role.RoleName + "/DataDisks";
            var content = new XElement(XmlNamespaces.WindowsAzure + "DataVirtualHardDisk");
            content.Add(new XElement(XmlNamespaces.WindowsAzure + "HostCaching", HostCaching.ToString()),
                        new XElement(XmlNamespaces.WindowsAzure + "DiskLabel", DiskLabel),
                        new XElement(XmlNamespaces.WindowsAzure + "Lun", Lun),
                        new XElement(XmlNamespaces.WindowsAzure + "LogicalDiskSizeInGB", LogicalDiskSizeInGB),
                        new XElement(XmlNamespaces.WindowsAzure + "MediaLink", MediaLink));


            var client = GetRestClient(suffix);
            var response = await client.PostAsync(content);
            await Role.Deployment.GetCloudService().Subscription.WaitForOperationCompletionAsync(response);
        }

        internal void AssignDiskName(string name)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));
            DiskName = name;
        }

        internal void AssignMediaLink(string mediaLink)
        {
            Contract.Requires(!String.IsNullOrEmpty(mediaLink));
            MediaLink = mediaLink;
        }

        public void AssignLun(int lun)
        {
            Contract.Requires(lun >= 0 && lun <= 31);
            Lun = lun;
        }

        public void AssignLogicalDiskSizeInGB(int sizeInGb)
        {
            Contract.Requires(sizeInGb > 0);
            LogicalDiskSizeInGB = sizeInGb;
        }

        internal void AssignHostCaching(HostCaching caching)
        {
            HostCaching = caching;
        }

        public async Task DeleteDiskAsync(bool deleteSourceBlob)
        {
            Contract.Requires(Role != null);
            var suffix = Role.Deployment.Name + "/roles/" + Role.RoleName + "/DataDisks/" + Lun;

            var client = GetRestClient(suffix,deleteSourceBlob ? "?comp=media" : String.Empty);
            var response = await client.DeleteAsync();
            await Role.Deployment.GetCloudService().Subscription.WaitForOperationCompletionAsync(response);
        }

        private AzureRestClient GetRestClient(string suffix = "", string queryString = "")
        {
            var cloudService = Role.Deployment.GetCloudService();
            var servicePath = "services/hostedservices/" + cloudService.Name + "/deployments/" + suffix + queryString;
            var client = cloudService.Subscription.GetDatabaseRestClient(servicePath);
            return client;
        }

        [Ignore]
        public Role Role { get; private set; }
        public HostCaching HostCaching { get; private set; }
        public string DiskLabel { get; private set; }
        public string DiskName { get; private set; }
        public int? Lun { get; private set; }
        public long? LogicalDiskSizeInGB { get; private set; }
        public string MediaLink { get; private set; }
        public string SourceMediaLink { get; private set; }
    }
}