using System;

namespace Linq2Azure.SqlDatabases
{
    internal enum Edition
    {
        [Obsolete]
        Web,
        Basic,
        Standard,
        Premium,
        [Obsolete]
        Business
    }
}