using System;

namespace Linq2Azure.CloudServices
{
    public interface IDeployment
    {
        string Name { get; }
        string Label { get; }
        DeploymentSlot Slot { get; }
        Lazy<bool> IsVirtualMachineDeployment { get; }
        CloudService GetCloudService();
    }
}