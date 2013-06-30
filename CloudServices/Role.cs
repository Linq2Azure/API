using System.Xml.Linq;

namespace Linq2Azure.CloudServices
{
    public class Role
    {
        public bool IsVirtualMachine { get; private set; }
        public string Name { get; private set; }

        internal Role(XElement roleData)
        {
            Name = (string) roleData.Element(XmlNamespaces.WindowsAzure + "RoleName");
            IsVirtualMachine = (string)roleData.Attribute(XmlNamespaces.SchemaInstance + "type") == "PersistentVMRole";
        }
    }
}