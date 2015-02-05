using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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
        public List<ResourceExtensionReference> ResourceExtensionReferences { get; set; }
        public List<DataVirtualHardDisk> DataVirtualHardDisks { get; set; }
        public OSVirtualHardDisk OsVirtualHardDisk { get; set; }
    }

    public class OSVirtualHardDisk
    {
        public HostCaching HostCaching { get; set; }
        public string DiskLabel { get; set; }
        public string DiskName { get; set; }
        public string MediaLink { get; set; }
        public string SourceImageName { get; set; }
        public string OS { get; set; }
        public string ResizedSizeInGB { get; set; }
        public string RemoteSourceImageLink { get; set; }
    }

    public class DataVirtualHardDisk
    {
        public HostCaching HostCaching { get; set; }
        public string DiskLabel { get; set; }
        public string DiskName { get; set; }
        public string Lun { get; set; }
        public string LogicalDiskSizeInGB { get; set; }
        public string MediaLink { get; set; }
        public string SourceMediaLink { get; set; }
    }

    public class ResourceExtensionReference
    {
        public string ResourceExtensionReference_ { get; set; }
        public string ReferenceName { get; set; }
        public string Publisher { get; set; }
        public string name { get; set; }
        public string Version { get; set; }
        public List<ResourceExtensionParameterValue> ResourceExtensionParameterValues { get; set; }
        public string State { get; set; }
        public List<Certificate> Certificates { get; set; }
    }

    public class Certificate
    {
        public string Thumbprint { get; set; }
        public string ThumbprintAlgorithm { get; set; }
    }

    public class ResourceExtensionParameterValue
    {
        public string ResourceExtensionParameterValue_ { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public String Type { get; set; }
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
        public WinRM WinRM { get; set; }
        public string AdminUsername { get; set; }
        public string CustomData { get; set; }
        public AdditionalUnattendedContent AdditionalUnattendedContent { get; set; }
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public bool DisableSshPasswordAuthentication { get; set; }
        public SSH SSH { get; set; }
        public List<InputEndpoint> InputEndpoints { get; set; }
        public List<string> SubnetNames { get; set; }
        public string StaticVirtualNetworkIPAddress { get; set; }
        public List<PublicIP> PublicIPs { get; set; } 
    }

    public class PublicIP
    {
        public string PublicIP_ { get; set; }
        public string Name { get; set; }
        public int IdleTimeoutInMinutes { get; set; }
    }

    public class InputEndpoint
    {
        public string InputEndpoint_ { get; set; }
        public string LoadBalancedEndpointSetName { get; set; }
        public string LocalPort { get; set; }
        public string Name { get; set; }
        public string Port { get; set; }
        public LoadBalancerProbe LoadBalancerProbe { get; set; }
        public string Protocol { get; set; }
        public bool EnableDirectServerReturn { get; set; }
        public EndpointACL EndpointACL { get; set; }
        public string LoadBalancerName { get; set; }
        public int IdleTimeoutInMinutes { get; set; }
    }

    public class EndpointACL
    {

    }

    public class LoadBalancerProbe
    {
        public string Path { get; set; }
        public string Port { get; set; }
        public string Protocol { get; set; }
        public int? IntervalInSeconds { get; set; }
        public int? TimeoutInSeconds { get; set; }
    }

    public class SSH
    {
    }

    public class AdditionalUnattendedContent
    {
        public string PassName { get; set; }
        public string ComponentName { get; set; }
        public string SettingName { get; set; }
        public string Content { get; set; }
    }

    public class WinRM
    {

        public string Protocol { get; set; }
        public string CertificateThumbprint { get; set; }
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