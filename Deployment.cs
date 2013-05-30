using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;

namespace Linq2Azure
{
    public class Deployment
    {
        public string Name { get; private set; }
        public string Url { get; private set; }
        public DeploymentSlot Slot { get; private set; }
        public string PrivateID { get; private set; }
        public string Label { get; set; }
        public ServiceConfiguration Configuration { get; set; }

        public CloudService Parent { get; private set; }

        public Deployment(string deploymentName, DeploymentSlot deploymentSlot, ServiceConfiguration serviceConfig)
        {
            Contract.Requires(deploymentName != null);
            Contract.Requires(serviceConfig != null);

            Name = Label = deploymentName;
            Slot = deploymentSlot;
            Configuration = serviceConfig;
        }

        internal Deployment(XElement element, CloudService parent)
        {
            Contract.Requires(element != null);
            Contract.Requires(parent != null);

            Parent = parent;
            PopulateFromXml(element);
        }

        void PopulateFromXml(XElement element)
        {
            Name = (string)element.Element(XmlNamespaces.Base + "Name");
            Url = (string)element.Element(XmlNamespaces.Base + "Url");
            Slot = (DeploymentSlot)Enum.Parse(typeof(DeploymentSlot), (string)element.Element(XmlNamespaces.Base + "DeploymentSlot"), true);
            PrivateID = (string)element.Element(XmlNamespaces.Base + "PrivateID");
            Label = ((string)element.Element(XmlNamespaces.Base + "Label")).FromBase64String();
            Configuration = new ServiceConfiguration(XElement.Parse(element.Element(XmlNamespaces.Base + "Configuration").Value.FromBase64String()));
        }

        public async Task CreateAsync(CloudService parent, Uri packageUrl, CreationOptions options = null)
        {
            Contract.Requires(parent != null);
            Contract.Requires(packageUrl != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(Label));
            Contract.Requires(Configuration != null);

            if (options == null) options = new CreationOptions();
            var ns = XmlNamespaces.Base;
            var content = new XElement(ns + "CreateDeployment",
                new XElement(ns + "Name", Name),
                new XElement(ns + "PackageUrl", packageUrl.ToString()),
                new XElement(ns + "Label", Label.ToBase64String()),
                new XElement(ns + "Configuration", Configuration.ToXml().ToString().ToBase64String()),
                new XElement(ns + "StartDeployment", options.StartDeployment),
                new XElement(ns + "TreatWarningsAsError", options.TreatWarningsAsError)
                );

            HttpResponseMessage response = await GetRestClient(parent).PostAsync(content);
            await parent.Subscription.WaitForOperationCompletionAsync(response);
            Parent = parent;
        }

        public async Task Refresh()
        {
            Contract.Requires(Parent != null);
            XElement xe = await GetRestClient().GetXmlAsync();
            PopulateFromXml(xe);
        }

        public async Task UpdateConfiguration()
        {
            Contract.Requires(Parent != null);
            
            var ns = XmlNamespaces.Base;
            var content = new XElement(ns + "ChangeConfiguration",
                new XElement(ns + "Configuration", Configuration.ToXml().ToString().ToBase64String()));

            HttpResponseMessage response = await GetRestClient(Parent, "?comp=config").PostAsync(content);
            await Parent.Subscription.WaitForOperationCompletionAsync(response);
        }

        public Task StartAsync()
        {
            Contract.Requires(Parent != null);
            return UpdateDeploymentStatus("Running");
        }

        public Task StopAsync()
        {
            Contract.Requires(Parent != null);
            return UpdateDeploymentStatus("Suspended");
        }

        async Task UpdateDeploymentStatus(string status)
        {
            var ns = XmlNamespaces.Base;
            var content = new XElement(ns + "UpdateDeploymentStatus", new XElement(ns + "Status", status));
            HttpResponseMessage response = await GetRestClient("?comp=status").PostAsync(content);
            await Parent.Subscription.WaitForOperationCompletionAsync(response);
        }

        public async Task DeleteAsync()
        {
            Contract.Requires(Parent != null);
            await Parent.Subscription.WaitForOperationCompletionAsync(await GetRestClient().DeleteAsync());
            Parent = null;
        }

        public IObservable<RoleInstance> GetRoleInstances()
        {
            return GetRoleInstancesAsync().ToObservable().SelectMany(x => x);
        }

        public async Task<RoleInstance[]> GetRoleInstancesAsync()
        {
            Contract.Requires(Parent != null);
            XElement xe = await GetRestClient().GetXmlAsync();
            return xe.Element(XmlNamespaces.Base + "RoleInstanceList")
                .Elements(XmlNamespaces.Base + "RoleInstance")
                .Select(r => new RoleInstance(r))
                .ToArray();
        }

        AzureRestClient GetRestClient(string queryString = null) { return GetRestClient(Parent, queryString); }

        AzureRestClient GetRestClient(CloudService cloudService, string queryString = null)
        {
            string uri = "services/hostedservices/" + cloudService.Name + "/deploymentslots/" + Slot.ToString().ToLowerInvariant();
            // With the deployments endpoint, you need a forward slash separating the URI from the query string!
            if (!string.IsNullOrEmpty(queryString)) uri += "/" + queryString;
            return cloudService.Subscription.GetRestClient(uri);
        }

        public class CreationOptions
        {
            public bool StartDeployment { get; set; }
            public bool TreatWarningsAsError { get; set; }
        }
    }

    public enum DeploymentSlot { Production, Staging }
}