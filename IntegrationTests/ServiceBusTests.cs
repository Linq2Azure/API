using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Linq2Azure;
using Linq2Azure.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class ServiceBusTests : IDisposable
    {
        public const string Password = "gj3eowl%5fi:edf";

        public readonly Subscription Subscription = TestConstants.Subscription;
        public readonly ServiceBusNamespace ServiceBusNamespace = new ServiceBusNamespace("testadmin", "West US");

        public ServiceBusTests()
        {
            Debug.WriteLine("Creating Service bus namespace...");
            Subscription.CreateServiceBusNamespaceAsync(ServiceBusNamespace).Wait();
        }

        [TestMethod]
        public async Task CanGetNameSpaces()
        {
            var namespaces = (await Subscription.ServiceBusNamespaces.AsTask()).Single(d => d.Name == ServiceBusNamespace.Name);

            Assert.IsNotNull(namespaces);
        }

        public void Dispose()
        {
            if (ServiceBusNamespace.Subscription == null)
            {
                return;
            }
            Debug.WriteLine("Dropping namespace...");
            ServiceBusNamespace.DeleteAsync().Wait();

            VerifyDeletion();
        }

        private void VerifyDeletion()
        {
            Debug.WriteLine("Verifying service bus namespace deletion...");
            Assert.IsNull(Subscription.ServiceBusNamespaces.AsArray().SingleOrDefault(d => d.Name == ServiceBusNamespace.Name));
        }
    }
}