namespace Linq2Azure.VirtualMachines
{
    public class Administrator
    {
        protected Administrator(string value)
        {
            Value = value;
        }

        public static Administrator Is(string administrator)
        {
            return new Administrator(administrator);
        }

        public string Value { get; private set; }
    }
}