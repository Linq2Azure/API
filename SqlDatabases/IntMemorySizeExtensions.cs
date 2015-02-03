namespace Linq2Azure.SqlDatabases
{
    public static class IntMemorySizeExtensions
    {
        public static int Megabytes(this int value)
        {
            return value * 1024 * 1024;
        }

        public static long Gigabytes(this int value)
        {
            return (long)value * 1024 * 1024 * 1024;
        }
    }
}