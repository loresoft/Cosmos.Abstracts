using System.Linq;
using System.Threading.Tasks;

using Bogus;

using Cosmos.Abstracts.Extensions;
using Cosmos.Abstracts.Tests.Models;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace Cosmos.Abstracts.Tests;

public class TemplateRepositoryTest : TestServiceBase
{
    public TemplateRepositoryTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task FullTest()
    {
        var generator = new Faker<Template>()
            .RuleFor(p => p.Id, _ => ObjectId.GenerateNewId().ToString())
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.OwnerId, f => f.PickRandom(Constants.Owners));

        var item = generator.Generate();

        var partitionKey = item.GetPartitionKey();
        partitionKey.Should().NotBeNull();
        partitionKey.ToString().Should().Be($"[\"{item.OwnerId}\"]");

        var repository = Services.GetRequiredService<ICosmosRepository<Template>>();
        repository.Should().NotBeNull();

        // create
        var createResult = await repository.CreateAsync(item);
        createResult.Should().NotBeNull();
        createResult.Id.Should().Be(item.Id);

        // read
        var readResult = await repository.FindAsync(item.Id, item.GetPartitionKey());
        readResult.Should().NotBeNull();
        readResult.Id.Should().Be(item.Id);
        readResult.OwnerId.Should().Be(item.OwnerId);

        // update
        readResult.Name = "Big " + readResult.Name;

        var updateResult = await repository.UpdateAsync(readResult);
        updateResult.Should().NotBeNull();
        updateResult.Id.Should().Be(item.Id);
        updateResult.OwnerId.Should().Be(item.OwnerId);

        // query
        var queryResult = await repository.FindOneAsync(r => r.Name.StartsWith("Big"));
        queryResult.Should().NotBeNull();

        var queryResults = await repository.FindAllAsync(r => r.Name.StartsWith("Big"));
        queryResults.Should().NotBeNull();
        queryResults.Count.Should().BeGreaterThan(0);

        // delete
        await repository.DeleteAsync(readResult);

        var deletedResult = await repository.FindAsync(item.Id, item.GetPartitionKey());
        deletedResult.Should().BeNull();
    }


    [Fact]
    public async Task QueryableTest()
    {
        var repository = Services.GetRequiredService<ICosmosRepository<Template>>();
        repository.Should().NotBeNull();

        var generator = new Faker<Template>()
            .RuleFor(p => p.Id, _ => ObjectId.GenerateNewId().ToString())
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.OwnerId, f => f.PickRandom(Constants.Owners));

        var item = generator.Generate();

        // create
        var createResult = await repository.CreateAsync(item);
        createResult.Should().NotBeNull();
        createResult.Id.Should().Be(item.Id);

        var queryable = await repository.GetQueryableAsync();
        queryable.Should().NotBeNull();

        var items = queryable.Take(10).ToListAsync();
        items.Should().NotBeNull();

        var firstOrDefault = queryable.FirstOrDefaultAsync();
        firstOrDefault.Should().NotBeNull();

        var first = queryable.FirstAsync(p => p.Id == item.Id);
        first.Should().NotBeNull();
    }
}
