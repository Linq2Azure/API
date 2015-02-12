using System.Threading.Tasks;

namespace Linq2Azure.VirtualMachines
{
    public interface IVirtualMachineBuilder
    {
        IRoleBuilder AddRole(string roleName, RoleSize size = RoleSize.Small);
        Task Provision();
    }
}