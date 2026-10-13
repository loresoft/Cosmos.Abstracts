using System.Threading.Tasks;

using Cosmos.Abstracts.Tests.Models;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace Cosmos.Abstracts.Tests;

public class CommentRepositoryTest(DatabaseFixture databaseFixture) : DatabaseTestBase(databaseFixture)
{
    [Fact]
    public async Task FullTest()
    {
        var generator = new Faker<Comment>()
            .RuleFor(p => p.Id, _ => ObjectId.GenerateNewId().ToString())
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.OwnerId, f => f.PickRandom(Constants.Owners));

        var item = generator.Generate();

        var partitionKey = new PartitionKey(item.OwnerId);
        partitionKey.Should().NotBeNull();

        var repository = Services.GetRequiredService<ICosmosRepository<Comment>>();
        repository.Should().NotBeNull();


        // create
        var createResult = await repository.CreateAsync(item, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        createResult.Should().NotBeNull();
        createResult.Id.Should().Be(item.Id);

        // read
        var readResult = await repository.FindAsync(item.Id, partitionKey, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        readResult.Should().NotBeNull();
        readResult.Id.Should().Be(item.Id);
        readResult.OwnerId.Should().Be(item.OwnerId);

        // update
        readResult.Name = "Big " + readResult.Name;

        var updateResult = await repository.UpdateAsync(readResult, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        updateResult.Should().NotBeNull();
        updateResult.Id.Should().Be(item.Id);
        updateResult.OwnerId.Should().Be(item.OwnerId);

        // query
        var queryResult = await repository.FindOneAsync(r => r.Name.StartsWith("Big"), cancellationToken: Xunit.TestContext.Current.CancellationToken);
        queryResult.Should().NotBeNull();

        var queryResults = await repository.FindAllAsync(r => r.Name.StartsWith("Big"), cancellationToken: Xunit.TestContext.Current.CancellationToken);
        queryResults.Should().NotBeNull();
        queryResults.Count.Should().BeGreaterThan(0);

        // delete
        await repository.DeleteAsync(readResult, cancellationToken: Xunit.TestContext.Current.CancellationToken);

        var deletedResult = await repository.FindAsync(item.Id, partitionKey, cancellationToken: Xunit.TestContext.Current.CancellationToken);
        deletedResult.Should().BeNull();
    }
}
