using System.Collections.Generic;

namespace Linq2Azure.VirtualMachines
{
    public class ResourceExtensionReference
    {
        public string ResourceExtensionReference_ { get; set; }
        public string ReferenceName { get; set; }
        public string Publisher { get; set; }
        public string name { get; set; }
        public string Version { get; set; }
        public List<ResourceExtensionParameterValue> ResourceExtensionParameterValues { get; set; }
        public string State { get; set; }
        public List<Certificate> Certificates { get; set; }
    }
}