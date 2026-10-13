using XUnit.Hosting;

namespace Cosmos.Abstracts.Tests;

[Collection(DatabaseCollection.CollectionName)]
public abstract class DatabaseTestBase(DatabaseFixture databaseFixture) : TestHostBase<DatabaseFixture>(databaseFixture)
{
}
