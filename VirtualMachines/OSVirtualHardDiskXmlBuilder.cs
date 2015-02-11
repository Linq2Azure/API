using System;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class OSVirtualHardDiskXmlBuilder
    {
        private readonly bool _isWindows;
        public OSVirtualHardDisk OSVirtualHardDisk { get; set; }

        public OSVirtualHardDiskXmlBuilder(OSVirtualHardDisk osVirtualHardDisk, bool isWindows)
        {
            _isWindows = isWindows;
            OSVirtualHardDisk = osVirtualHardDisk;
        }

        public XElement Create()
        {
            var element = new XElement(XmlNamespaces.WindowsAzure + "OSVirtualHardDisk");

            if(!String.IsNullOrEmpty(OSVirtualHardDisk.DiskLabel))
                element.Add(new XElement(XmlNamespaces.WindowsAzure + "DiskLabel", OSVirtualHardDisk.DiskLabel));

            if(!String.IsNullOrEmpty(OSVirtualHardDisk.DiskName))
                element.Add(new XElement(XmlNamespaces.WindowsAzure + "DiskName", OSVirtualHardDisk.DiskName));

            if(OSVirtualHardDisk.HostCaching.HasValue)
                element.Add(new XElement(XmlNamespaces.WindowsAzure + "HostCaching", OSVirtualHardDisk.HostCaching));


            if (_isWindows)
            {
                AddSourceImageName(element);
                AddMediaLink(element);
            }
            else
            {
                AddMediaLink(element);
                AddSourceImageName(element);
            }

            return element;
        }

        private void AddMediaLink(XElement element)
        {
            if (!String.IsNullOrEmpty(OSVirtualHardDisk.MediaLink))
                element.Add(new XElement(XmlNamespaces.WindowsAzure + "MediaLink", OSVirtualHardDisk.MediaLink));
        }

        private void AddSourceImageName(XElement element)
        {
            if (!String.IsNullOrEmpty(OSVirtualHardDisk.SourceImageName))
                element.Add(new XElement(XmlNamespaces.WindowsAzure + "SourceImageName",
                    OSVirtualHardDisk.SourceImageName));
        }
    }
}