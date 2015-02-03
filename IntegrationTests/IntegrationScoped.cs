using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    public class IntegrationScoped<TStrategy> where TStrategy : IScopedStrategy, new()
    {

        protected readonly TStrategy Strategy = new TStrategy();

        [TestInitialize]
        public void Setup()
        {
            Strategy.Setup().Wait();
        }

        [TestCleanup]
        public void Teardown()
        {
            Strategy.Teardown().Wait();
        }

    }
}