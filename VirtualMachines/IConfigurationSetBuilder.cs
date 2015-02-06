using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public interface IConfigurationSetBuilder
    {
        XElement Create();
    }
}