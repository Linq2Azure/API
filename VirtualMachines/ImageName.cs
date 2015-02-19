using System;
using System.Diagnostics.Contracts;

namespace Linq2Azure.VirtualMachines
{
    public class ImageName
    {
        protected ImageName(string name)
        {
            OsName = name;
        }

        public static ImageName Named(string name)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));
            return new ImageName(name);
        }

        public string OsName { get; private set; }
    }
}