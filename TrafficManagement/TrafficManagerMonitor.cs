using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.TrafficManagement
{
    public class TrafficManagerMonitor
    {
        public int IntervalInSeconds { get; set; }
        public int TimeoutInSeconds { get; set; }
        public int ToleratedNumberOfFailures { get; set; }
        public MonitorProtocol Protocol { get; set; }
        public int Port { get; set; }
        public TrafficManagerHttpOptions HttpOptions { get; set; }

        public TrafficManagerMonitor()
        {
            IntervalInSeconds = 30;
            TimeoutInSeconds = 10;
            ToleratedNumberOfFailures = 3;
        }

        public TrafficManagerMonitor(int port, TrafficManagerHttpOptions httpOptions) : this()
        {
            Port = port;
            HttpOptions = httpOptions;
        }

        internal TrafficManagerMonitor(XElement xml)
        {
            var ns = XmlNamespaces.WindowsAzure;
            xml.HydrateObject(ns, this);
            HttpOptions = new TrafficManagerHttpOptions(xml.Element(ns + "HttpOptions"));
        }

        internal XElement ToXml()
        {
            var ns = XmlNamespaces.WindowsAzure;
            return new XElement(ns + "Monitor",
                new XElement(ns + "IntervalInSeconds", IntervalInSeconds),
                new XElement(ns + "TimeoutInSeconds", TimeoutInSeconds),
                new XElement(ns + "ToleratedNumberOfFailures", ToleratedNumberOfFailures),
                new XElement(ns + "Protocol", Protocol.ToString()),
                new XElement(ns + "Port", Port),
                HttpOptions.ToXml());
        }
    }

    public class TrafficManagerHttpOptions
    {
        public string RelativePath { get; set; }
        public string Verb { get; set; }
        public int ExpectedStatusCode { get; set; }

        public TrafficManagerHttpOptions()
        {
            Verb = "GET";
            ExpectedStatusCode = 200;
        }

        public TrafficManagerHttpOptions(string relativePath) : this()
        {
            RelativePath = relativePath;
        }

        internal TrafficManagerHttpOptions(XElement xml)
        {
            xml.HydrateObject(XmlNamespaces.WindowsAzure, this);
        }

        internal XElement ToXml()
        {
            var ns = XmlNamespaces.WindowsAzure;
            return new XElement(ns + "HttpOptions",
                new XElement(ns + "Verb", Verb),
                new XElement(ns + "RelativePath", RelativePath),
                new XElement(ns + "ExpectedStatusCode", ExpectedStatusCode));
        }
    }

    public enum MonitorProtocol { HTTP, HTTPS }
}
