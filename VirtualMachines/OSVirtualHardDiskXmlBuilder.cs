using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class OSVirtualHardDiskXmlBuilder
    {
        public OSVirtualHardDisk OSVirtualHardDisk { get; set; }

        public OSVirtualHardDiskXmlBuilder(OSVirtualHardDisk osVirtualHardDisk)
        {
            OSVirtualHardDisk = osVirtualHardDisk;
        }

        public XElement Create()
        {
            var element = new XElement(XmlNamespaces.WindowsAzure + "OSVirtualHardDisk",
               // new XElement(XmlNamespaces.WindowsAzure + "HostCaching", OSVirtualHardDisk.HostCaching),
               // new XElement(XmlNamespaces.WindowsAzure + "DiskLabel", OSVirtualHardDisk.DiskLabel),
              //  new XElement(XmlNamespaces.WindowsAzure + "DiskName", OSVirtualHardDisk.DiskName),
                new XElement(XmlNamespaces.WindowsAzure + "SourceImageName", OSVirtualHardDisk.SourceImageName),
                new XElement(XmlNamespaces.WindowsAzure + "MediaLink", OSVirtualHardDisk.MediaLink)
                );

            return element;
        }
    }
}