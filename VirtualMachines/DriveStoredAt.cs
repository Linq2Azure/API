using System;
using System.Diagnostics.Contracts;

namespace Linq2Azure.VirtualMachines
{
    public class DriveStoredAt
    {
        protected DriveStoredAt(Uri location)
        {
            Location = location;
        }

        public static DriveStoredAt LocatedAt(Uri location)
        {
            Contract.Requires(location != null);
            return new DriveStoredAt(location);
        }

        public Uri Location { get; private set; }
    }
}