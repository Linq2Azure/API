using System;
using System.Diagnostics.Contracts;

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
            Contract.Requires(!String.IsNullOrEmpty(name));
            return new Os(name);
        }

        public string OsName { get; private set; }
    }
}