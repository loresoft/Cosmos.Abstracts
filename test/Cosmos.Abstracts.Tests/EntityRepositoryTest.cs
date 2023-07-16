using System;

using Cosmos.Abstracts.Tests.Models;

using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

namespace Cosmos.Abstracts.Tests;

public class EntityRepositoryTest : TestServiceBase
{
    public EntityRepositoryTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void TestGet()
    {
        var entity = new Entry
        {
            Id = ObjectId.GenerateNewId().ToString(),
            EntryDate = new DateTimeOffset(2020, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var partitionKey = entity.GetPartitionKey();
        partitionKey.Should().NotBeNull();

        var key = partitionKey.ToString();
        key.Should().Be("[\"1/1/2020 12:00:00 PM +00:00\"]");
    }
}
