using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using Linq2Azure;

namespace UnitTests
{
    [TestClass]
    public class TestServiceConfiguration
    {
        [TestMethod]
        public void TestSerialization()
        {
            var xmlString = @"<ServiceConfiguration serviceName=""TestDeployment"" osFamily=""1"" osVersion=""*"" schemaVersion=""2012-05.1.7"" xmlns=""http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration"">
  <Role name=""TestWorkerRole"" vmsize=""ExtraSmall"">
    <Instances count=""1"" />
    <ConfigurationSettings>
      <Setting name=""Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"" value=""UseDevelopmentStorage=true"" />
    </ConfigurationSettings>
  </Role>
</ServiceConfiguration>";
            var xe = XElement.Parse(xmlString);
            var rolesConfig = new ServiceConfiguration(xe);

            Assert.AreEqual(xe.ToString(), rolesConfig.ToXml().ToString());
        }
    }
}
