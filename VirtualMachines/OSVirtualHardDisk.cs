using System;
using System.Diagnostics.Contracts;

namespace Linq2Azure.VirtualMachines
{
    public class OSVirtualHardDisk
    {

        public OSVirtualHardDisk()
        {
            HostCaching = HostCaching.None;
        }

        public OSVirtualHardDisk(string label, HostCaching caching = HostCaching.None)
        {
            Contract.Requires(!String.IsNullOrEmpty(label));
            DiskLabel = label;
            HostCaching = caching;
        }

        public void AssignMedia(string sourceImageName, string mediaLInk)
        {
            Contract.Requires(!String.IsNullOrEmpty(sourceImageName));
            Contract.Requires(!String.IsNullOrEmpty(mediaLInk));

            SourceImageName = sourceImageName;
            MediaLink = mediaLInk;
        }

        public void ChangeHostCaching(HostCaching caching)
        {
            HostCaching = caching;
        }

        public void AssignDiskName(string name)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));
            DiskName = name;
        }

        public HostCaching HostCaching { get; private set; }
        public string DiskLabel { get; private set; }
        public string DiskName { get; private set; }
        public string MediaLink { get; private set; }
        public string SourceImageName { get; private set; }
        public OsType OS { get; private set; }
        public string ResizedSizeInGB { get; private set; }
        public string RemoteSourceImageLink { get; private set; }

        
    }
}