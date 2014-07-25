using System;
using System.Xml.Linq;

namespace Linq2Azure.SqlDatabases
{
    public class Database
    {
        internal Database(XElement xml, DatabaseServer databaseServer)
        {
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
            DatabaseServer = databaseServer;
        }

        public DatabaseServer DatabaseServer { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string State { get; private set; }
        public Uri SelfLink { get; private set; }
        public Uri ParentLink { get; private set; }
        public int Id { get; private set; }
        public string Edition { get; private set; }
        public string CollationName { get; private set; }
        public DateTimeOffset CreationDate { get; private set; }
        public bool IsFederationRoot { get; private set; }
        public bool IsSystemObject { get; private set; }
        public decimal? SizeMB { get; private set; }
        public long MaxSizeBytes { get; private set; }
        public Guid ServiceObjectiveId { get; private set; }
        public Guid AssignedServiceObjectiveId { get; private set; }
        public int ServiceObjectiveAssignmentState { get; private set; }
        public string ServiceObjectiveAssignmentStateDescription { get; private set; }
        public int ServiceObjectiveAssignmentErrorCode { get; private set; }
        public string ServiceObjectiveAssignmentErrorDescription { get; private set; }
        public DateTimeOffset ServiceObjectiveAssignmentSuccessDate { get; private set; }
        public DateTimeOffset? RecoveryPeriodStartDate { get; private set; }
        public bool IsSuspended { get; private set; }
    }
}