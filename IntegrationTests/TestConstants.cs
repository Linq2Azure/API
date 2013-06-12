using Linq2Azure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IntegrationTests
{
    public static class TestConstants
    {
        public const string ManagementCertificatePath = @"c:\temp\Linq2Azure Development-5-27-2013-credentials.publishsettings";

        public static readonly Subscription Subscription = new Subscription(TestConstants.ManagementCertificatePath);

        public static readonly Uri TestDeploymentPackageUri = new Uri(@"http://linq2azuredev.blob.core.windows.net/testdata/TestDeployment.cspkg");

        public static readonly string TestServiceConfigString = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ServiceConfiguration serviceName=""WindowsAzure1"" xmlns=""http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration"" osFamily=""2"" osVersion=""*"" schemaVersion=""2012-10.1.8"">
  <Role name=""TestWebRole"">
    <Instances count=""1"" />
    <ConfigurationSettings>
      <Setting name=""Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"" value=""UseDevelopmentStorage=true"" />
    </ConfigurationSettings>
    <Certificates>
      <Certificate name=""Certificate1"" thumbprint=""85D7BCFA0AA2343CF1F8BCE144C5BFF8C0BD9506"" thumbprintAlgorithm=""sha1"" />
    </Certificates>
  </Role>
</ServiceConfiguration>";

        public static readonly XElement TestServiceConfig = XElement.Parse(TestServiceConfigString);

        static TestConstants()
        {
            Subscription.LogDestinations = Debug.Listeners.Cast<TraceListener>();
        }
    }
}
