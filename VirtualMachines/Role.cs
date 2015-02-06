using System.Collections.Generic;

namespace Linq2Azure.VirtualMachines
{
    public class Role
    {
        public Role()
        {
            ConfigurationSets = new List<ConfigurationSet>();
            ResourceExtensionReferences = new List<ResourceExtensionReference>();
            DataVirtualHardDisks = new List<DataVirtualHardDisk>();
            OsVirtualHardDisk = new OSVirtualHardDisk();
            VmImageInput = new VImageInput();
        }

        public string RoleName { get; set; }
        public string RoleType { get; set; }
        public List<ConfigurationSet> ConfigurationSets { get; set; }
        public string VMImageName { get; set; }
        public string MediaLocation { get; set; }
        public string AvailabilitySetName { get; set; }
        public List<ResourceExtensionReference> ResourceExtensionReferences { get; set; }
        public List<DataVirtualHardDisk> DataVirtualHardDisks { get; set; }
        public OSVirtualHardDisk OsVirtualHardDisk { get; set; }
        public string RoleSize { get; set; }
        public bool ProvisionGuessAgent { get; set; }
        public VImageInput VmImageInput { get; set; }
    }
}