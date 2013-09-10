using System.Xml.Linq;
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
    }
}