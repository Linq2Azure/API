namespace Linq2Azure.VirtualMachines
{
    public class Password
    {

        protected Password(string value)
        {
            Value = value;
        }

        public static Password Is(string password)
        {
            return new Password(password);
        }

        public string Value { get; private set; }

    }
}