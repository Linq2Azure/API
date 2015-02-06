namespace Linq2Azure.VirtualMachines
{
    public interface INetworkConfigurationSetBuilder
    {
        INetworkConfigurationSetBuilder AddWebPort();
        INetworkConfigurationSetBuilder AddRemoteDesktop();
    }
}