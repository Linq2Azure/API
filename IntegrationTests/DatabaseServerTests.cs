using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linq2Azure;
using System.Threading.Tasks;
using System.Diagnostics;

namespace IntegrationTests
{
    [TestClass]
    public class DatabaseServerTests
    {
        [TestMethod]
        public async Task TestCreateDelete()
        {
            var sub = TestConstants.Subscription;
            var server = new DatabaseServer("testadmin", "West US");
            
            Debug.WriteLine("Creating server...");
            await server.CreateAsync(sub, "gj3eowl%5fi:edf");
            Assert.IsNotNull(server.Name);

            Debug.WriteLine("Updating password...");
            await server.UpdateAdminPassword("gj3eowl%5fi:edf2");

            Debug.WriteLine("Dropping server...");
            await server.DropAsync();            
        }
    }
}
