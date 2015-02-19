using System;
using System.Threading.Tasks;

namespace Linq2Azure.VirtualMachines
{
    public interface IGuidedSpecificDataDiskConfiguration
    {
        IGuidedSpecificDataDiskConfiguration WithAdditionalDiskSettings(Action<AdditionalDiskSettings> settings);
        IDataDiskConfigurationBuilder AddDisk(DiskLabel label);
        Task Provision();
    }
}