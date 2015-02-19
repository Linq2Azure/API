using System;
using System.Diagnostics.Contracts;

namespace Linq2Azure.VirtualMachines
{
    public class DiskLabel
    {
        protected DiskLabel(string label)
        {
            Label = label;
        }

        public static DiskLabel Is(string label)
        {
            Contract.Requires(!String.IsNullOrEmpty(label));
            return new DiskLabel(label);
        }

        public string Label { get; private set; }
    }
}