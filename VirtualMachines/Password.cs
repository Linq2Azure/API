using System;
using System.Diagnostics.Contracts;

namespace Linq2Azure.VirtualMachines
{
    public class Password
    {

        protected Password(string value)
        {

            Contract.Requires(!String.IsNullOrEmpty(value));

            Value = value;
        }

        public static Password Is(string password)
        {
            return new Password(password);
        }

        public string Value { get; private set; }

    }
}