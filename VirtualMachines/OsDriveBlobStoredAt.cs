using System;

namespace Linq2Azure.VirtualMachines
{
    public class OsDriveBlobStoredAt
    {
        protected OsDriveBlobStoredAt(Uri location)
        {
            Location = location;
        }

        public static OsDriveBlobStoredAt LocatedAt(Uri location)
        {
            return new OsDriveBlobStoredAt(location);
        }

        public Uri Location { get; private set; }
    }
}