using System;
using System.Diagnostics.Contracts;

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
            Contract.Requires(!String.IsNullOrEmpty(hostname));
            return new Hostname(hostname);
        }

        public string Value { get; private set; }
    }
}