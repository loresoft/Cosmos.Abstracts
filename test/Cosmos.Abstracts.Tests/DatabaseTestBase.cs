﻿using Xunit;
using Xunit.Abstractions;

using XUnit.Hosting;

namespace Cosmos.Abstracts.Tests;

[Collection(DatabaseCollection.CollectionName)]
public abstract class DatabaseTestBase : TestHostBase<DatabaseFixture>
{
    protected DatabaseTestBase(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
    {
    }
}
