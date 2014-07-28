using System;

namespace Linq2Azure
{
    public class AffinityGroupDeploymentAssociation : IDeploymentAssociation
    {
        public AffinityGroupDeploymentAssociation(string affinityGroup)
        {
            AffinityGroup = affinityGroup;
        }

        public string AffinityGroup { get; private set; }

        public void AssignValue(Action<string> assignLocation, Action<string> assignAffinityGroup)
        {
            assignAffinityGroup(AffinityGroup);
        }
    }
}