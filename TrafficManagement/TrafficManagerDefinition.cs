using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.TrafficManagement
{
    public class TrafficManagerDefinition
    {
        public int DnsTtlInSeconds { get; set; }
        public bool Enabled { get; set; }
        public TrafficManagerProfile Parent { get; private set; }
        public List<TrafficManagerMonitor> Monitors { get; set; }
        public TrafficManagerPolicy Policy { get; set; }

        public TrafficManagerDefinition()
        {
            Monitors = new List<TrafficManagerMonitor>();
            Policy = new TrafficManagerPolicy();
            Enabled = true;
        }

        public TrafficManagerDefinition(int dnsTtlInSeconds, IEnumerable<TrafficManagerMonitor> monitors, TrafficManagerPolicy policy) : this()
        {
            DnsTtlInSeconds = dnsTtlInSeconds;
            Monitors = monitors.ToList();
            Policy = policy;
        }

        internal TrafficManagerDefinition(XElement xml, TrafficManagerProfile parent)
        {
            var ns = XmlNamespaces.WindowsAzure;
            DnsTtlInSeconds = (int)xml.Element(ns + "DnsOptions").Element(ns + "TimeToLiveInSeconds");
            Enabled = (string)xml.Element(ns + "Status") != "Disabled";
            Monitors = xml.Element(ns + "Monitors").Elements(ns + "Monitor").Select(xe => new TrafficManagerMonitor(xe)).ToList();
            Policy = new TrafficManagerPolicy(xml.Element(ns + "Policy"));
            Parent = parent;
        }

        public async Task CreateAsync(TrafficManagerProfile parent)
        {
            Contract.Requires(Parent == null);
            Contract.Requires(parent != null);

            var ns = XmlNamespaces.WindowsAzure;
            var content = new XElement(ns + "Definition",
                new XElement(ns + "DnsOptions", new XElement (ns + "TimeToLiveInSeconds", DnsTtlInSeconds)),
                new XElement (ns + "Monitors", Monitors.Select (m => m.ToXml())),
                Policy.ToXml());

            await GetRestClient(parent).PostAsync(content);
            Parent = parent;
        }

        AzureRestClient GetRestClient(string pathSuffix = null) { return GetRestClient(Parent, pathSuffix); }

        AzureRestClient GetRestClient(TrafficManagerProfile profile, string pathSuffix = null)
        {
            string servicePath = "/" + profile.Name + "/definitions";
            if (!string.IsNullOrEmpty(pathSuffix)) servicePath += pathSuffix;
            return profile.GetRestClient(servicePath);
        }
    }
}
