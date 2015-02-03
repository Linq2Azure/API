using System.Threading.Tasks;

namespace IntegrationTests
{
    public interface IScopedStrategy
    {
        Task Setup();
        Task Teardown();
    }
}