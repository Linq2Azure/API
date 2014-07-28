using System;

namespace Linq2Azure
{
    public class LocationDeploymentAssociation : IDeploymentAssociation
    {
        public LocationDeploymentAssociation(string location)
        {
            Location = location;
        }

        public string Location { get; private set; }

        public void AssignValue(Action<string> assignLocation, Action<string> assignAffinityGroup)
        {
            assignLocation(Location);
        }
    }
}