using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class When_creating_a_database_and_live_replica : ReplicaDatabaseSetup
    {

        [TestMethod]
        public void It_should_have_the_replica_in_its_replicas_collection()
        {
            Database.CreateLiveReplica(DestinationServer).Wait();
            Assert.AreEqual(1, DatabaseServer.Databases.AsArray().SingleOrDefault(x => x.Name == DatabaseName).Replicas.AsArray().Count());
        }

    }
}