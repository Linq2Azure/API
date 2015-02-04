using System;

namespace Linq2Azure.SqlDatabases
{
    internal class ServiceTierMetadataAttribute : Attribute
    {
        public Edition Edition { get; private set; }
        public Guid PerformanceLevel { get; set; }

        public ServiceTierMetadataAttribute(Edition edition, string performanceLevel)
        {
            Edition = edition;
            PerformanceLevel = String.IsNullOrEmpty(performanceLevel) ? Guid.Empty : new Guid(performanceLevel);
        }
    }
}