using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_creating_a_database_with_valid_credentials : IntegrationScoped<StandardDatabaseStrategy>
    {

        [TestMethod]
        public void It_should_have_the_database_in_its_databases_collection()
        {
            Assert.AreEqual(1, Strategy.DatabaseServer.Databases.AsArray().Count(x => x.Name == Strategy.DatabaseName));
        }

    }
}