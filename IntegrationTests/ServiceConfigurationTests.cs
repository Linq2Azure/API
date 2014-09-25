using System;
using System.Linq;

using ApprovalTests;
using ApprovalTests.Reporters;

using Linq2Azure.CloudServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    [UseReporter(typeof(DiffReporter))]
    public class ServiceConfigurationTests
    {
        public static string XmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ServiceConfiguration serviceName=""Hoover.Worker.CloudService"" osFamily=""4"" osVersion=""*"" schemaVersion=""2014-06.2.4"" xmlns=""http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration"" >
  <Role name=""Hoover.Worker"">
    <NotAnAzureElement>
      <Unknown attribute=""2""/>
    </NotAnAzureElement>
    <Instances count=""1"" />
    <ConfigurationSettings>
      <Setting name=""Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"" value=""DefaultEndpointsProtocol=https;AccountName=hooverdevstorage;AccountKey=2AlsQGY39ZqB8mRzFvJR2o+o8YtIH94E+TlWO+AzhI3Kei5ryJIgzv1ug/rtDBCmpn6rvg9Lr/X8HIQcUK8xUA=="" />
      <Setting name=""DbConnectionString"" value=""Server=tcp:ir8i4ca1z8.database.windows.net,1433;Database=Hoover;User ID=DevTeam@ir8i4ca1z8;Password=71d785fc@F*!hH;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"" />
      <Setting name=""YodleeApi.BaseUrl"" value=""https://sdkint11.yodlee.com/yodsoap/srest/private-cashconverters/v1.0/"" />
      <Setting name=""YodleeApi.CobrandUsername"" value=""private-cashconverters"" />
      <Setting name=""YodleeApi.CobrandPassword"" value=""C1!a5@s2#h9&amp;"" />
      <Setting name=""Statement.Storage"" value=""DefaultEndpointsProtocol=https;AccountName=hooverdevstorage;AccountKey=2AlsQGY39ZqB8mRzFvJR2o+o8YtIH94E+TlWO+AzhI3Kei5ryJIgzv1ug/rtDBCmpn6rvg9Lr/X8HIQcUK8xUA=="" />
      <Setting name=""AdminSiteBaseUrl"" value=""https://ccstatementadmindev.cashconverters.com.au/"" />
      <Setting name=""OnlineTeamEmailAddress"" value=""nick.little@cashconverters.com"" />
      <Setting name=""YodleeApi.ShouldPassDate"" value=""true"" />
      <Setting name=""StatementSubmissionUrl"" value=""http://au.staging.safrock.net/fw/remote/index.cfm/DocumentGateWay/Listener/CCStatement"" />
      <Setting name=""Environment"" value=""Dev"" />
      <Setting name=""StatementRetrievalToken"" value=""94f389fc-7820-404e-8a31-1320f457bf39"" />
    </ConfigurationSettings>
    <Certificates>
      <Certificate name=""Test"" thumbprint=""12345"" thumbprintAlgorithm=""RSA""/>
    </Certificates>   </Role>
  <NetworkConfiguration>
    <AddressAssignments>
      <ReservedIPs>
        <ReservedIP name=""HooverDevWorkerIP""/>
      </ReservedIPs>
    </AddressAssignments>
  </NetworkConfiguration>
 
  <NotAnAzureElement>
    <Unknown attribute=""2""/>
  </NotAnAzureElement>
</ServiceConfiguration>";

        [TestMethod]
        public void WillNotLoseUnknownValuesDuringParsing()
        {
            Approvals.VerifyXml(new ServiceConfiguration(XmlText).ToXml().ToString());
        }

        [TestMethod]
        public void CanSetServiceName()
        {
            var serviceConfiguration = new ServiceConfiguration(XmlText) {ServiceName = "NewServiceName"};

            Approvals.VerifyXml(serviceConfiguration.ToXml().ToString());
        }

        [TestMethod]
        public void CanSetOsFamily()
        {
            var serviceConfiguration = new ServiceConfiguration(XmlText) {OsFamily = 12};

            Approvals.VerifyXml(serviceConfiguration.ToXml().ToString());
        }

        [TestMethod]
        public void CanSetOsVersion()
        {
            var serviceConfiguration = new ServiceConfiguration(XmlText) {OsVersion = "blah"};

            Approvals.VerifyXml(serviceConfiguration.ToXml().ToString());
        }

        [TestMethod]
        public void CanSetRoleName()
        {
            var serviceConfiguration = new ServiceConfiguration(XmlText);
            serviceConfiguration.Roles.First().RoleName = "RandomName";

            Approvals.VerifyXml(serviceConfiguration.ToXml().ToString());
        }

        [TestMethod]
        public void CanSetRoleInstanceCount()
        {
            var serviceConfiguration = new ServiceConfiguration(XmlText);
            serviceConfiguration.Roles.First().InstanceCount = 145;

            Approvals.VerifyXml(serviceConfiguration.ToXml().ToString());
        }

        [TestMethod]
        public void CanSetConfigurationSettings()
        {
            var serviceConfiguration = new ServiceConfiguration(XmlText);
            serviceConfiguration.Roles.First().ConfigurationSettings["DbConnectionString"] = "notaconnectionstring";

            Approvals.VerifyXml(serviceConfiguration.ToXml().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotSetUnknownSetting()
        {
            var serviceConfiguration = new ServiceConfiguration(XmlText);
            serviceConfiguration.Roles.First().ConfigurationSettings["Unknown"] = "notaconnectionstring";
        }

        [TestMethod]
        public void CanSetCertificateThumbprint()
        {
            var serviceConfiguration = new ServiceConfiguration(XmlText);
            serviceConfiguration.Roles.First().Certificates["Test"].Thumbprint = "64544";

            Approvals.VerifyXml(serviceConfiguration.ToXml().ToString());
        }

        [TestMethod]
        public void CanSetCertificateThumbprintAlgorithm()
        {
            var serviceConfiguration = new ServiceConfiguration(XmlText);
            serviceConfiguration.Roles.First().Certificates["Test"].ThumbprintAlgorithm = "SHA";

            Approvals.VerifyXml(serviceConfiguration.ToXml().ToString());
        }
    }
}