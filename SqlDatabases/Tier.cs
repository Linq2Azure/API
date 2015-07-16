using System;

namespace Linq2Azure.SqlDatabases
{
    internal class Tier
    {
        public Tier(ServiceTier serviceTier)
        {
            var attribute = serviceTier.GetAttributeOfType<ServiceTierMetadataAttribute>();
            Edition = attribute.Edition;

            PerformanceLevel = attribute.PerformanceLevel;
        }

        public Edition Edition { get; private set; }
        public Guid PerformanceLevel { get; private set; }
    }
}