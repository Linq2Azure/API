using System;
using System.Threading.Tasks;

namespace Linq2Azure.VirtualMachines
{
    public interface IVirtualMachineBuilder
    {
        IRoleBuilder AddRole(string roleName, RoleSize size = RoleSize.Small);
        IRoleBuilder AddRole(string roleName, RoleSize roleSize, Action<AdditionalRoleSettings> f);
        Task Provision();
    }


}