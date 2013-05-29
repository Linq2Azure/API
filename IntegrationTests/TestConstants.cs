using Linq2Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IntegrationTests
{
    public class TestConstants
    {
        public const string ManagementCertificatePath = @"c:\temp\Linq2Azure Development-5-27-2013-credentials.publishsettings";
        public static readonly Subscription Subscription = Subscription.FromPublisherSettingsPath(TestConstants.ManagementCertificatePath);
        public static readonly Uri TestDeploymentPackageUri = new Uri (@"http://linq2azuredev.blob.core.windows.net/testdata/TestDeployment.cspkg");

        public static readonly XElement TestServiceConfig = XElement.Parse (@"<ServiceConfiguration serviceName=""TestDeployment"" osFamily=""1"" osVersion=""*"" schemaVersion=""2012-05.1.7"" xmlns=""http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration"">
  <Role name=""TestWorkerRole"" vmsize=""ExtraSmall"">
    <Instances count=""1"" />
    <ConfigurationSettings>
      <Setting name=""Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"" value=""UseDevelopmentStorage=true"" />
    </ConfigurationSettings>
  </Role>
</ServiceConfiguration>");
    }
}
