using System.Collections.Generic;
using System.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class AdditionalRoleSettings
    {
        public AdditionalRoleSettings(Subscription subscription)
        {
            Subscription = subscription;
            Names = new List<string>();
            Extensions = new List<ResourceExtensionReference>();
        }

        public AdditionalRoleSettings AddVS14CTPDebugger()
        {
            Names.Add("VS14CTPDebugger");
            return this;
        }

        public AdditionalRoleSettings AddVS2012Debugger()
        {
            Names.Add("VS2012Debugger");
            return this;
        }

        public AdditionalRoleSettings AddVS2013Debugger()
        {
            Names.Add("VS2013Debugger");
            return this;
        }

        public AdditionalRoleSettings AddSystemCenter()
        {
            Names.Add("MSEnterpriseApplication");
            return this;
        }

        public AdditionalRoleSettings AddDocker()
        {
            Names.Add("DockerExtension");
            return this;
        }

        public AdditionalRoleSettings AddPowershell()
        {
            Names.Add("DSC");
            return this;
        }

        public AdditionalRoleSettings AddWebDeploy()
        {
            Names.Add("WebDeployForVSDevTest");
            return this;
        }

        public AdditionalRoleSettings AddAntiMalware()
        {
            Names.Add("IaaSAntimalware");
            return this;
        }

        public AdditionalRoleSettings AddMcAfee()
        {
            Names.Add("McAfeeEndpointSecurity");
            return this;
        }

        public AdditionalRoleSettings AddBGInfo()
        {
            Names.Add("BGInfo");
            return this;
        }

        public AdditionalRoleSettings AddLinuxOSPatching()
        {
            Names.Add("OSPatchingForLinux");
            return this;
        }

        public void Build()
        {
            var resourceExtensionReferences = Subscription.ResourceExtensionReferences.AsArray().Where(x => Names.Contains(x.Name));

            if (resourceExtensionReferences.Any())
                Extensions.AddRange(resourceExtensionReferences);
        }

        private List<string> Names { get; set; }
        private Subscription Subscription { get; set; }
        public bool ProvisionGuestAgent { get; set; }
        public string AvailabilitySetName { get; set; }
        public List<ResourceExtensionReference> Extensions { get; set; }

        
    }


}