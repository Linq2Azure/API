namespace Linq2Azure.VirtualMachines
{
    public class DomainJoin
    {
        public Credentials Credentials { get; set; }
        public string JoinDomain { get; set; }
        public string MachineObjectOU { get; set; }
    }
}