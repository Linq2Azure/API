using System.Configuration;
using System.Xml.Linq;
using System.IO;
using Linq2Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{   
    [TestClass]
    public class TestSubscription
    {
        [TestMethod]
        [ExpectedException(typeof(AzureRestException))]
        public void CanParseErrorResponses()
        {
            var result = XElement.Parse(
            @"<Operation xmlns=""http://schemas.microsoft.com/windowsazure"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                <ID>id</ID>
                <Status>Failed</Status>
                <HttpStatusCode>400</HttpStatusCode>
                <Error>
                    <Code>BadRequest</Code>
                    <Message>This is the error message</Message>
                </Error>
              </Operation>");
            
            Subscription.ParseResult(result);
        }

        [TestMethod]
        public void CanExtractStatusFromResponse()
        {
            var result = XElement.Parse(
            @"<Operation xmlns=""http://schemas.microsoft.com/windowsazure"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
                <ID>id</ID>
                <Status>TheStatus</Status>
                <HttpStatusCode>200</HttpStatusCode>
              </Operation>");

            var parsed = Subscription.ParseResult(result);
            Assert.AreEqual("TheStatus", parsed);
        }

        [TestMethod]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void CanInitialize()
        {
            string content = 
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
        <PublishData>
          <PublishProfile
            SchemaVersion=""2.0""
            PublishMethod=""AzureServiceManagementAPI"">
            <Subscription
              ServiceManagementUrl=""https://management.core.windows.net""
              Id="""+System.Guid.Empty+@"""
              Name=""MSDN""/>
          </PublishProfile>
        </PublishData>";

            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filename = "SubscriptionAttempt.xml";
            string filepath = Path.Combine(path, filename);

            System.IO.File.WriteAllText(filepath, content, System.Text.Encoding.UTF8);

            try
            {
                var attempt = new Subscription(filepath);
            }
            finally
            {
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }
            }
        }



    }
}