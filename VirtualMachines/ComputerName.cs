using System;
using System.Diagnostics.Contracts;

namespace Linq2Azure.VirtualMachines
{
    public class ComputerName
    {
        protected ComputerName(string value)
        {
            Value = value;
        }

        public static ComputerName Is(string computerName)
        {
            Contract.Requires(!String.IsNullOrEmpty(computerName));
            return new ComputerName(computerName);
        }

        public string Value { get; private set; }
    }
}