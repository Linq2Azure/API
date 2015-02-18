using System;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class DataVirtualHardDiskXmlBuilder
    {
        public DataVirtualHardDisk DataVirtualHardDisk { get; set; }

        public DataVirtualHardDiskXmlBuilder(DataVirtualHardDisk dataVirtualHardDisk)
        {
            DataVirtualHardDisk = dataVirtualHardDisk;
        }

        public XElement Create()
        {
            var element = new XElement(XmlNamespaces.WindowsAzure + "reference",
                new XElement(XmlNamespaces.WindowsAzure + "HostCaching", DataVirtualHardDisk.HostCaching.ToString()),
                new XElement(XmlNamespaces.WindowsAzure + "DiskLabel", DataVirtualHardDisk.DiskLabel)
                );

            if (!String.IsNullOrEmpty(DataVirtualHardDisk.DiskName))
                element.Add(new XElement(XmlNamespaces.WindowsAzure + "DiskName", DataVirtualHardDisk.DiskName));

            if (DataVirtualHardDisk.LogicalDiskSizeInGB.HasValue)
                element.Add(new XElement(XmlNamespaces.WindowsAzure + "LogicalDiskSizeInGB",DataVirtualHardDisk.LogicalDiskSizeInGB));

            if (!String.IsNullOrEmpty(DataVirtualHardDisk.MediaLink))
                element.Add(new XElement(XmlNamespaces.WindowsAzure + "MediaLink", DataVirtualHardDisk.MediaLink));

            return element;
        }
    }
}