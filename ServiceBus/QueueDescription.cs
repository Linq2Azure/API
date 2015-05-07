using System;
using System.Xml.Linq;

namespace Linq2Azure.ServiceBus
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect", IsNullable = false)]
    public class QueueDescription
    {
        public ServiceBusNamespace ServiceBusNamespace { get; set; }

        public QueueDescription(ServiceBusNamespace serviceBusNamespace, XElement element)
        {
            ServiceBusNamespace = serviceBusNamespace;
            PopulatedSelf(element);
        }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "duration")]
        public string LockDuration { get; set; }

        public ushort MaxSizeInMegabytes { get; set; }

        public bool RequiresDuplicateDetection { get; set; }

        public bool RequiresSession { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "duration")]
        public string DefaultMessageTimeToLive { get; set; }

        public bool DeadLetteringOnMessageExpiration { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "duration")]
        public string DuplicateDetectionHistoryTimeWindow { get; set; }

        public byte MaxDeliveryCount { get; set; }

        public bool EnableBatchedOperations { get; set; }

        public byte SizeInBytes { get; set; }

        public byte MessageCount { get; set; }

        public bool IsAnonymousAccessible { get; set; }

        public object AuthorizationRules { get; set; }//?

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public DateTime AccessedAt { get; set; }

        public bool SupportOrdering { get; set; }

        public QueueDescriptionCountDetails CountDetails { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "duration")]
        public string AutoDeleteOnIdle { get; set; }

        public bool EnablePartitioning { get; set; }

        public string EntityAvailabilityStatus { get; set; }

        public bool EnableExpress { get; set; }

        private void PopulatedSelf(XElement element)
        {
            element.HydrateObject(XmlNamespaces.ServiceBusConfig, this);
            CreatedAt = ((DateTime)element.Element(XmlNamespaces.ServiceBusConfig + "CreatedAt"));
            UpdatedAt = ((DateTime)element.Element(XmlNamespaces.ServiceBusConfig + "UpdatedAt"));
            AccessedAt = ((DateTime)element.Element(XmlNamespaces.ServiceBusConfig + "AccessedAt"));
            CountDetails = new QueueDescriptionCountDetails(element);
        }
    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect")]
    public class QueueDescriptionCountDetails
    {
        public QueueDescriptionCountDetails(XElement element)
        {
            element.HydrateObject(XmlNamespaces.ServiceBusConfig, this);
        }

        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/netservices/2011/06/servicebus")]
        public byte ActiveMessageCount { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/netservices/2011/06/servicebus")]
        public byte DeadLetterMessageCount { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/netservices/2011/06/servicebus")]
        public byte ScheduledMessageCount { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/netservices/2011/06/servicebus")]
        public byte TransferMessageCount { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://schemas.microsoft.com/netservices/2011/06/servicebus")]
        public byte TransferDeadLetterMessageCount { get; set; }
    }
}