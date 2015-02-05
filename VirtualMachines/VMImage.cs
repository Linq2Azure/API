using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Linq2Azure.VirtualMachines
{
    public class VMImage
    {

        internal VMImage(XElement xml, Subscription subscription)
        {
            Subscription = subscription;
            DataDiskConfigurations = new List<DataDiskConfiguration>();
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
        }

        public Subscription Subscription { get; private set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }

        [Traverse]
        public OSDiskConfiguration OSDiskConfiguration { get; set; }

        [Traverse]
        public List<DataDiskConfiguration> DataDiskConfigurations { get; set; }

        public string ServiceName { get; set; }
        public string DeploymentName { get; set; }
        public string RoleName { get; set; }
        public string Location { get; set; }
        public string AffinityGroup { get; set; }
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset ModifiedTime { get; set; }
        public string Language { get; set; }
        public string ImageFamily { get; set; }
        public string RecommendedVMSize { get; set; }
        public bool IsPremium { get; set; }
        public string Eula { get; set; }
        public string IconUri { get; set; }
        public string SmalIconUri { get; set; }
        public string PrivacyUri { get; set; }
        public DateTimeOffset PublishDate { get; set; }

    }

    public class Deployment
    {

        public string Name { get; set; }
        public string DeploymentSlot { get; set; }
        public string Label { get; set; }
        public List<Role> Role { get; set; }
        public string VirtualNetworkName { get; set; }
        public string ReservedIPName { get; set; }
        
        
    }

    public class Role
    {
        public string RoleName { get; set; }
        public string RoleType { get; set; }
        public List<ConfigurationSet> ConfigurationSets { get; set; }
        public string VMImageName { get; set; }
        public string MediaLocation { get; set; }
        public string AvailabilitySetName { get; set; }
    }

    public class ConfigurationSet
    {
        public string ConfigurationSetType { get; set; }
        public string ComputerName { get; set; }
        public string AdminPassword { get; set; }
        public bool EnableAutomaticUpdates { get; set; }
        public string TimeZone { get; set; }
        public DomainJoin DomainJoin { get; set; }
        public StoredCertificateSettings StoredCertificateSettings { get; set; }
    }

    public class StoredCertificateSettings
    {
        public string CertificateSetting { get; set; }
        public string StoreLocation { get; set; }
        public string StoreName { get; set; }
        public string Thumbprint { get; set; }
    }

    public class DomainJoin
    {
        public Credentials Credentials { get; set; }
        public string JoinDomain { get; set; }
        public string MachineObjectOU { get; set; }
    }

    public class Credentials
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}