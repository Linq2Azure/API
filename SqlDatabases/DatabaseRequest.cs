namespace Linq2Azure.SqlDatabases
{
    public class DatabaseRequest 
    {

        public DatabaseRequest(string databaseName, Edition edition, PerformanceLevel performance, string collationName,long maximumBytes)
        {
            Name = databaseName;
            Edition = edition;
            Performance = performance;
            CollationName = collationName;
            MaxBytes = maximumBytes;
        }

        public string Name { get; private set; }
        public Edition Edition { get; private set; }
        public string CollationName { get; private set; }
        public long MaxBytes { get; private set; }
        public PerformanceLevel Performance { get; private set; }

    }
}