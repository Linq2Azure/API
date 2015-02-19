using System;
using System.Diagnostics.Contracts;

namespace Linq2Azure.VirtualMachines
{
    public class DataDiskConfiguration
    {

        public DataDiskConfiguration()
        {
            HostCaching = HostCaching.None;
            Lun = 0;
            LogicalDiskSizeInGB = 1;
        }

        internal DataDiskConfiguration(HostCaching caching, string name, int lun, string mediaLink, int logicalDiskSizeInGb)
        {

            Contract.Requires(logicalDiskSizeInGb > 0);
            Contract.Requires(lun >= 0 && lun <= 31);
            Contract.Requires(!String.IsNullOrEmpty(mediaLink));

            HostCaching = caching;
            Name = name;
            Lun = lun;
            MediaLink = mediaLink;
            LogicalDiskSizeInGB = logicalDiskSizeInGb;
        }

        public string Name { get; private set; }
        public HostCaching HostCaching { get; private set; }
        public int Lun { get; private set; }
        public string MediaLink { get; private set; }
        public int LogicalDiskSizeInGB { get; private set; }
        public string IOType { get; private set; }
    }
}