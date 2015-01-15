using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.CloudServices
{
    public class Deployment
    {
        public string Name { get; private set; }
        public string Url { get; private set; }
        public DeploymentSlot Slot { get; private set; }
        public string PrivateID { get; private set; }
        public string Label { get; set; }
        public ServiceConfiguration Configuration { get; set; }
        public LatentSequence<RoleInstance> RoleInstances { get; private set; }
        public CloudService Parent { get; private set; }

        private Deployment()
        {
            RoleInstances = new LatentSequence<RoleInstance>(GetRoleInstancesAsync);
        }

        public Deployment(string deploymentName, DeploymentSlot deploymentSlot, string serviceConfig)
            : this(deploymentName, deploymentSlot, new ServiceConfiguration(serviceConfig))
        {
        }

        public Deployment(string deploymentName, DeploymentSlot deploymentSlot, XElement serviceConfig)
            : this(deploymentName, deploymentSlot, new ServiceConfiguration(serviceConfig))
        {
        }

        public Deployment(string deploymentName, DeploymentSlot deploymentSlot, ServiceConfiguration serviceConfig)
            : this()
        {
            Contract.Requires(deploymentName != null);
            Contract.Requires(serviceConfig != null);

            Name = Label = deploymentName;
            Slot = deploymentSlot;
            Configuration = serviceConfig;
        }

        internal Deployment(XElement element, CloudService parent)
            : this()
        {
            Contract.Requires(element != null);
            Contract.Requires(parent != null);

            Parent = parent;
            PopulateFromXml(element);
        }

        void PopulateFromXml(XElement element)
        {
            element.HydrateObject(XmlNamespaces.WindowsAzure, this);
            Slot = (DeploymentSlot)Enum.Parse(typeof(DeploymentSlot), (string)element.Element(XmlNamespaces.WindowsAzure + "DeploymentSlot"), true);
            if (!string.IsNullOrEmpty(Label)) Label = Label.FromBase64String();
            Configuration = new ServiceConfiguration(XElement.Parse(element.Element(XmlNamespaces.WindowsAzure + "Configuration").Value.FromBase64String()));
        }

        internal async Task CreateAsync(
            CloudService parent,
            Uri packageUrl,
            CreationOptions options = null,
            params ExtensionAssociation[] extensionAssociations)
        {
            Contract.Requires(parent != null);
            Contract.Requires(packageUrl != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(Label));
            Contract.Requires(Configuration != null);

            if (options == null) options = new CreationOptions();
            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "CreateDeployment",
                new XElement(ns + "Name", Name),
                new XElement(ns + "PackageUrl", packageUrl.ToString()),
                new XElement(ns + "Label", Label.ToBase64String()),
                new XElement(ns + "Configuration", Configuration.ToXml().ToString().ToBase64String()),
                new XElement(ns + "StartDeployment", options.StartDeployment),
                new XElement(ns + "TreatWarningsAsError", options.TreatWarningsAsError)
                );

            AddExtensionConfigurationXml(content, extensionAssociations);

            var response = await GetRestClient(parent).PostAsync(content);
            await parent.Subscription.WaitForOperationCompletionAsync(response);
            Parent = parent;
        }

        public async Task RefreshAsync()
        {
            Contract.Requires(Parent != null);
            XElement xe = await GetRestClient().GetXmlAsync();
            PopulateFromXml(xe);
        }

        /// <summary>
        /// Submits any changes to the Configuration property.
        /// </summary>
        /// <param name="extensionAssociations">
        ///     Optional extensions to be adjusted against the deployment
        /// </param>
        public async Task UpdateConfigurationAsync(params ExtensionAssociation[] extensionAssociations)
        {
            Contract.Requires(Parent != null);

            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "ChangeConfiguration");
            content.Add(new XElement(ns + "Configuration", Configuration.ToXml().ToString().ToBase64String()));

            AddExtensionConfigurationXml(content, extensionAssociations);

            // With the deployments endpoint, you need a forward slash separating the URI from the query string!
            var response = await GetRestClient(Parent, "/?comp=config").PostAsync(content);
            await Parent.Subscription.WaitForOperationCompletionAsync(response);
        }

        private static void AddExtensionConfigurationXml(
            XContainer parentElement,
            ExtensionAssociation[] extensionAssociations)
        {
            if (extensionAssociations.Length == 0)
            {
                return;
            }

            var ns = XmlNamespaces.WindowsAzure;

            var extensionConfigurationElement = new XElement(ns + "ExtensionConfiguration");
            parentElement.Add(extensionConfigurationElement);

            AddAllRolesExtensions(extensionAssociations, ns, extensionConfigurationElement);
            AddNamedRoleExtensions(extensionAssociations, ns, extensionConfigurationElement);
        }

        private static void AddAllRolesExtensions(ExtensionAssociation[] extensionAssociations, XNamespace ns, XElement extensionConfigurationElement)
        {
            var allRolesAssociations = extensionAssociations.OfType<AllRolesExtensionAssociation>().ToArray();

            if (allRolesAssociations.Length == 0)
            {
                return;
            }

            var allRolesElement = new XElement(ns + "AllRoles");

            var children = allRolesAssociations.Select(ara =>
                new XElement(
                    ns + "Extension",
                    new XElement(ns + "Id", ara.Id)));

            allRolesElement.Add(children);

            extensionConfigurationElement.Add(allRolesElement);
        }

        private static void AddNamedRoleExtensions(ExtensionAssociation[] extensionAssociations, XNamespace ns, XElement extensionConfigurationElement)
        {
            var namedRoleAssociations = extensionAssociations.OfType<NamedRoleExtensionAssociation>().ToArray();

            if (namedRoleAssociations.Length == 0)
            {
                return;
            }

            var namedRolesElement = new XElement(ns + "NamedRoles");
            extensionConfigurationElement.Add(namedRolesElement);

            foreach (var role in namedRoleAssociations.GroupBy(nra => nra.RoleName))
            {
                var roleElement = new XElement(
                    ns + "Role",
                    new XElement(ns + "RoleName", role.Key));
                var roleExtensionsElement = new XElement(ns + "Extensions");
                roleElement.Add(roleExtensionsElement);

                namedRolesElement.Add(roleElement);

                var children = role.Select(nra =>
                    new XElement(
                        ns + "Extension",
                        new XElement(ns + "Id", nra.Id)));

                roleExtensionsElement.Add(children);
            }
        }

        public Task StartAsync()
        {
            Contract.Requires(Parent != null);
            return UpdateDeploymentStatusAsync("Running");
        }

        public Task StopAsync()
        {
            Contract.Requires(Parent != null);
            return UpdateDeploymentStatusAsync("Suspended");
        }

        async Task UpdateDeploymentStatusAsync(string status)
        {
            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "UpdateDeploymentStatus", new XElement(ns + "Status", status));

            // With the deployments endpoint, you need a forward slash separating the URI from the query string!
            var response = await GetRestClient("/?comp=status").PostAsync(content);
            await Parent.Subscription.WaitForOperationCompletionAsync(response);
        }

        /// <summary>
        /// Upgrades the given deployment with the package contents.
        /// </summary>
        public Task UpgradeAsync(Uri packageUrl, string roleToUpgrade = null, UpgradeMode mode = UpgradeMode.Auto)
        {
            return UpgradeAsync(packageUrl, DeploymentType.Standard, roleToUpgrade, mode);
        }

        /// <summary>
        /// Upgrades the given deployment with the package contents.
        /// </summary>
        public async Task UpgradeAsync(
            Uri packageUrl,
            DeploymentType deploymentType,
            string roleToUpgrade = null,
            UpgradeMode mode = UpgradeMode.Auto)
        {
            Contract.Requires(Parent != null);
            Contract.Requires(packageUrl != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(Label));
            Contract.Requires(Configuration != null);
            
            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "UpgradeDeployment",
                new XElement(ns + "Mode", mode.ToString()),
                new XElement(ns + "PackageUrl", packageUrl.ToString()),
                new XElement(ns + "Configuration", Configuration.ToXml().ToString().ToBase64String()),
                new XElement(ns + "Label", Label.ToBase64String()),
                new XElement(ns + "RoleToUpgrade", roleToUpgrade),
                new XElement(ns + "Force", deploymentType == DeploymentType.Forced));
            // With the deployments endpoint, you need a forward slash separating the URI from the query string!
            HttpResponseMessage response = await GetRestClient(Parent, "/?comp=upgrade").PostAsync(content);
            await Parent.Subscription.WaitForOperationCompletionAsync(response);
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Parent != null);
            await Parent.Subscription.WaitForOperationCompletionAsync(await GetRestClient().DeleteAsync());
            Parent = null;
        }

        async Task<RoleInstance[]> GetRoleInstancesAsync()
        {
            Contract.Requires(Parent != null);
            var xe = await GetRestClient().GetXmlAsync();
            return xe.Element(XmlNamespaces.WindowsAzure + "RoleInstanceList")
                .Elements(XmlNamespaces.WindowsAzure + "RoleInstance")
                .Select(r => new RoleInstance(r, this))
                .ToArray();
        }

        internal AzureRestClient GetRestClient(string pathSuffix = null) { return GetRestClient(Parent, pathSuffix); }

        AzureRestClient GetRestClient(CloudService cloudService, string pathSuffix = null)
        {
            var servicePath = "services/hostedservices/" + cloudService.Name + "/deploymentslots/" + Slot.ToString().ToLowerInvariant();
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return cloudService.Subscription.GetCoreRestClient20141001(servicePath);
        }

        public class CreationOptions
        {
            public bool StartDeployment { get; set; }
            public bool TreatWarningsAsError { get; set; }
        }
    }

    public enum DeploymentSlot { Production, Staging }
    public enum UpgradeMode { Auto, Simultaneous }

    public abstract class ExtensionAssociation
    {
        public string Id { get; set; }
    }

    public class AllRolesExtensionAssociation : ExtensionAssociation
    {}

    public class NamedRoleExtensionAssociation : ExtensionAssociation
    {
        public string RoleName { get; set; }
    }
}