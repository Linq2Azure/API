using System;
using System.Diagnostics.Contracts;

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
            Contract.Requires(!String.IsNullOrEmpty(administrator));
            return new Administrator(administrator);
        }

        public string Value { get; private set; }
    }
}