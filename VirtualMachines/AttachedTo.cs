namespace Linq2Azure.VirtualMachines
{
    public class AttachedTo
    {

        protected AttachedTo()
        {
        }

        internal AttachedTo(string hostedServiceName, string deploymentName, string roleName)
        {
            HostedServiceName = hostedServiceName;
            DeploymentName = deploymentName;
            RoleName = roleName;
        }

        public string HostedServiceName { get; private set; }
        public string DeploymentName { get; private set; }
        public string RoleName { get; private set; }
    }
}