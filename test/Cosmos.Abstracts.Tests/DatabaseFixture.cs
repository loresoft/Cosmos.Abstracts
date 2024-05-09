using Cosmos.Abstracts.Tests.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using XUnit.Hosting;

namespace Cosmos.Abstracts.Tests;

public class DatabaseFixture : TestApplicationFixture
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        base.ConfigureApplication(builder);

        var services = builder.Services;

        services.AddCosmosRepository();

        services.TryAddSingleton<IUserRepository, UserRepository>();
    }
}
