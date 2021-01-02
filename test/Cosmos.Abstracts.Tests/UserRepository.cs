using Cosmos.Abstracts.Extensions;
using Cosmos.Abstracts.Tests.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cosmos.Abstracts.Tests
{
    public class UserRepository : CosmosRepository<User>
    {
        public UserRepository(ILoggerFactory logFactory, IOptions<CosmosRepositoryOptions> repositoryOptions, ICosmosFactory databaseFactory)
            : base(logFactory, repositoryOptions, databaseFactory)
        {
        }

        protected override void BeforeSave(User entity)
        {
            if (entity.Id.IsNullOrEmpty())
                entity.Id = entity.Email?.ToLowerInvariant();
        }
    }
}
