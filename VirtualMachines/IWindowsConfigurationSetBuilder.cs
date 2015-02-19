using System;

namespace Linq2Azure.VirtualMachines
{
    public interface IWindowsConfigurationSetBuilder
    {
        IWindowsConfigurationGuidedConfiguration WithAdditionalWindowsSettings(Action<AdditionalWindowsSettings> setting);
        INetworkConfigurationSetBuilder AddNetworkConfiguration();
    }
}