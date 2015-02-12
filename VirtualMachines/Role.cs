using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class Role
    {
        public Role()
        {
            ConfigurationSets = new List<ConfigurationSet>();
            ResourceExtensionReferences = new List<ResourceExtensionReference>();
            DataVirtualHardDisks = new List<DataVirtualHardDisk>();
            OSVirtualHardDisk = new OSVirtualHardDisk();
            VmImageInput = new VImageInput();
            RoleSize = RoleSize.Small;
        }

        public Role(XElement element)
            : this()
        {
            element.HydrateObject(XmlNamespaces.WindowsAzure, this);
         //   Debug.WriteLine(element);
        }

        public string RoleName { get; set; }
        public string RoleType { get; set; }

        [Traverse]
        public List<ConfigurationSet> ConfigurationSets { get; set; }
        public string VMImageName { get; set; }
        public string MediaLocation { get; set; }
        public string AvailabilitySetName { get; set; }

       // [Traverse]
        public List<ResourceExtensionReference> ResourceExtensionReferences { get; set; }

        [Traverse]
        public List<DataVirtualHardDisk> DataVirtualHardDisks { get; set; }

        [Traverse]
        public OSVirtualHardDisk OSVirtualHardDisk { get; set; }
        public RoleSize RoleSize { get; set; }
        public bool ProvisionGuessAgent { get; set; }

        //[Traverse]
        public VImageInput VmImageInput { get; set; }

        [Ignore]
        public bool OsVersion { get; set; }
    }
}