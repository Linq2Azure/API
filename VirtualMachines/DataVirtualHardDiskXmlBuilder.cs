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
            var element = new XElement(XmlNamespaces.WindowsAzure + "DataVirtualHardDisk",
                new XElement(XmlNamespaces.WindowsAzure + "HostCaching", DataVirtualHardDisk.HostCaching.ToString()),
                new XElement(XmlNamespaces.WindowsAzure + "DiskLabel", DataVirtualHardDisk.DiskLabel),
                new XElement(XmlNamespaces.WindowsAzure + "LogicalSizeInGB", DataVirtualHardDisk.LogicalDiskSizeInGB),
                new XElement(XmlNamespaces.WindowsAzure + "MediaLink", DataVirtualHardDisk.MediaLink)
                );

            return element;
        }
    }
}