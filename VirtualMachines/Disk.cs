using System;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class Disk
    {

        internal Disk(XElement xml, Subscription subscription)
        {
            Subscription = subscription;
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
        }

        public Subscription Subscription { get; set; }
        public string AffinityGroup { get; set; }

        [Traverse]
        public AttachedTo AttachedTo { get; set; }
        public string OS { get; set; }
        public string Location { get; set; }
        public string LogicalSizeInGB { get; set; }
        public string MediaLink { get; set; }
        public string Name { get; set; }
        public string SourceImageName { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string IOType { get; set; }

    }
}