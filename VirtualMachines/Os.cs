namespace Linq2Azure.VirtualMachines
{
    public class Os
    {
        protected Os(string name)
        {
            OsName = name;
        }

        public static Os Named(string name)
        {
            return new Os(name);
        }

        public string OsName { get; private set; }
    }
}