using Cosmos.Abstracts.Tests.Models;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace Cosmos.Abstracts.Tests;


public class RepositoryServiceTest : DatabaseTestBase
{
    public RepositoryServiceTest(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
    {
    }


    [Fact]
    public void DefaultServices()
    {
        var cosmosFactory = Services.GetRequiredService<ICosmosFactory>();
        cosmosFactory.Should().NotBeNull();
        cosmosFactory.Should().BeOfType<CosmosFactory>();

        var userRepo = Services.GetRequiredService<ICosmosRepository<User>>();
        userRepo.Should().NotBeNull();
        userRepo.Should().BeOfType<CosmosRepository<User>>();

        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();
        roleRepo.Should().BeOfType<CosmosRepository<Role>>();
    }
}
