using System;
using System.Linq;
using Linq2Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_listing_disks
    {
        public Subscription Subscription = null;

        [TestInitialize]
        public void TestInitialize()
        {
            Subscription = TestConstants.Subscription;
        }

        [TestMethod]
        public void it_should_have_disks()
        {
            var results = Subscription.VirtualMachineDisks.AsArray();

            foreach (var result in results)
            {
                Console.WriteLine(result);
            }

            Assert.AreNotEqual(0,results.Count());
        }
    }
}