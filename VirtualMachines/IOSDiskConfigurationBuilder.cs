namespace Linq2Azure.VirtualMachines
{
    public interface IOSDiskConfigurationBuilder
    {
        IRoleBuilder WithImage(string imageName);
    }
}