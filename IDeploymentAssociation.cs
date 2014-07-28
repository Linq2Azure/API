using System;

namespace Linq2Azure
{
    public interface IDeploymentAssociation
    {
        void AssignValue(Action<string> assignLocation, Action<string> assignAffinityGroup);
    }
}