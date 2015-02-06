using System.Collections.Generic;

namespace Linq2Azure.VirtualMachines
{
    public class VImageInput
    {
        public OSDiskConfiguration OSDiskConfiguration { get; set; }
        public List<DataDiskConfiguration> DataDiskConfigurations { get; set; } 
    }
}