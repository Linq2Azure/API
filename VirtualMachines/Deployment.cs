using System.Collections.Generic;

namespace Linq2Azure.VirtualMachines
{
    public class Deployment
    {
        public Deployment()
        {
            RoleList = new List<Role>();
        }

        public string Name { get; set; }
        public string DeploymentSlot { get; set; }
        public string Label { get; set; }
        public List<Role> RoleList { get; set; }
        public string VirtualNetworkName { get; set; }
        public string ReservedIPName { get; set; }
    }
}