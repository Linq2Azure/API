using System;

namespace Linq2Azure.VirtualMachines
{
    public interface ILinuxConfigurationSetBuilder
    {
        ILinuxConfigurationGuided WithAdditionalLinuxSettings(Action<AdditionaLinuxSettings> setting);
        INetworkConfigurationSetBuilder AddNetworkConfiguration();
        IDataDiskConfigurationBuilder AddDisk(DiskLabel @is);
    }
}