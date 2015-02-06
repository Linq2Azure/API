using System.Threading.Tasks;

namespace Linq2Azure.VirtualMachines
{
    public interface IVirtualMachineBuilder
    {
        IRoleBuilder AddRole(string roleName);
        Task Provision();
    }
}