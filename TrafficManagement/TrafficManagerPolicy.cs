using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.TrafficManagement
{
    public class TrafficManagerPolicy
    {
        public LoadBalancingMethod LoadBalancingMethod { get; set; }
        public List<TrafficManagerEndpoint> EndPoints { get; set; }

        public TrafficManagerPolicy()
        {
            EndPoints = new List<TrafficManagerEndpoint>();
        }

        public TrafficManagerPolicy(IEnumerable<TrafficManagerEndpoint> endpoints)
        {
            EndPoints = endpoints.ToList();
        }

        internal TrafficManagerPolicy(XElement xml)
        {
            var ns = XmlNamespaces.WindowsAzure;
            xml.HydrateObject(ns, this);
            EndPoints = xml.Element(ns + "Endpoints").Elements(ns + "Endpoint").Select(xe => new TrafficManagerEndpoint(xe)).ToList();
        }

        internal XElement ToXml()
        {
            var ns = XmlNamespaces.WindowsAzure;
            return new XElement(ns + "Policy",
                new XElement(ns + "LoadBalancingMethod", LoadBalancingMethod),
                new XElement(ns + "Endpoints", EndPoints.Select(ep => ep.ToXml())));
        }
    }

    public class TrafficManagerEndpoint
    {
        public string DomainName { get; set; }
        public bool Enabled { get; set; }

        public TrafficManagerEndpoint()
        {
            Enabled = true;
        }

        public TrafficManagerEndpoint(string domainName) : this()
        {
            DomainName = domainName;
        }

        internal TrafficManagerEndpoint(XElement xml)
        {
            var ns = XmlNamespaces.WindowsAzure;
            DomainName = (string)xml.Element(ns + "DomainName");
            Enabled = (string)xml.Element(ns + "Status") != "Disabled";
        }

        internal XElement ToXml()
        {
            var ns = XmlNamespaces.WindowsAzure;
            return new XElement(ns + "Endpoint",
                new XElement(ns + "DomainName", DomainName),
                new XElement(ns + "Status", Enabled ? "Enabled" : "Disabled"));
        }
    }

    public enum LoadBalancingMethod { Performance, Failover, RoundRobin }
}
