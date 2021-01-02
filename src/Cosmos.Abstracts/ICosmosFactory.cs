using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Cosmos.Abstracts
{
    public interface ICosmosFactory
    {
        CosmosClient GetCosmosClient();

        Task<Database> GetDatabaseAsync();
    }

}
