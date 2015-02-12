namespace Linq2Azure.VirtualMachines
{
    public class Hostname
    {
        protected Hostname(string value)
        {
            Value = value;
        }

        public static Hostname Is(string hostname)
        {
            return new Hostname(hostname);
        }

        public string Value { get; private set; }
    }
}