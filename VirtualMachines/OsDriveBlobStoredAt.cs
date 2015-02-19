using System;
using System.Diagnostics.Contracts;

namespace Linq2Azure.VirtualMachines
{
    public class OsDriveBlobStoredAt
    {
        protected OsDriveBlobStoredAt(Uri location)
        {

            Contract.Requires(location != null);

            Location = location;
        }

        public static OsDriveBlobStoredAt LocatedAt(Uri location)
        {
            return new OsDriveBlobStoredAt(location);
        }

        public Uri Location { get; private set; }
    }
}