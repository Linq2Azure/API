using Linq2Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    [TestClass]
    public class FirewallTests : IDisposable
    {
        public DatabaseServerTests Parent = new DatabaseServerTests();
        public DatabaseServer DatabaseServer { get { return Parent.DatabaseServer; } }

        [TestMethod]
        public async Task CanUseFirewall()
        {
            Debug.WriteLine ("Creating firewall rule");
            var rule = new FirewallRule("TestRule1", "0.0.0.0", "255.255.255.255");
            await rule.CreateAsync(DatabaseServer);

            Debug.WriteLine("Retrieving firewall rule");
            var retrievedRule = (await DatabaseServer.FirewallRules.AsTask()).SingleOrDefault(r => r.Name == "TestRule1");
            Assert.IsNotNull(retrievedRule);
            Assert.AreEqual(retrievedRule.StartIpAddress, rule.StartIpAddress);
            Assert.AreEqual(retrievedRule.EndIpAddress, rule.EndIpAddress);

            var cxString = "Data Source=tcp:" + DatabaseServer.Name + ".database.windows.net;User ID=testadmin;Password=" + DatabaseServerTests.Password +
                ";Initial Catalog=master;Encrypt=true";

            using (var cx = new SqlConnection(cxString))
            {
                Debug.WriteLine("Testing SQL connection");
                cx.Open();
                Assert.AreEqual(1, new SqlCommand("SELECT COUNT(*) FROM sys.firewall_rules", cx).ExecuteScalar());
            }

            Debug.WriteLine("Deleting firewall rule");
            await rule.DeleteAsync();

            Debug.WriteLine("Verifying firewall rule deletion");
            var deletedRule = (await DatabaseServer.FirewallRules.AsTask()).SingleOrDefault(r => r.Name == "TestRule1");
            Assert.IsNull(deletedRule);
        }

        public void Dispose()
        {
            Parent.Dispose();
        }
    }
}
