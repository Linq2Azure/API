using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_creating_a_database_and_live_replica: IntegrationScoped<ReplicaDatabaseStrategy>
    {

        [TestMethod]
        public void It_should_have_the_replica_in_its_replicas_collection()
        {
            Strategy.Database.CreateLiveReplica(Strategy.DestinationServer).Wait();
            Assert.AreEqual(1, Strategy.DatabaseServer.Databases.AsArray().SingleOrDefault(x => x.Name == Strategy.DatabaseName).Replicas.AsArray().Count());
        }

    }
}