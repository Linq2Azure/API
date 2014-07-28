namespace Linq2Azure
{
    public static class DeploymentAssociationExtensions
    {
        public static IDeploymentAssociation AsLocation(this string location)
        {
            return new LocationDeploymentAssociation(location);
        }

        public static IDeploymentAssociation AsAffinityGroup(this string affinityGroup)
        {
            return new AffinityGroupDeploymentAssociation(affinityGroup);
        }
    }
}