using Cosmos.Abstracts.Tests.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using XUnit.Hosting;

namespace Cosmos.Abstracts.Tests;

public class DatabaseFixture : TestHostFixture
{
    protected override void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        base.ConfigureLogging(context, builder);
        builder.SetMinimumLevel(LogLevel.Debug);
    }

    protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddCosmosRepository();

        services.TryAddSingleton<IUserRepository, UserRepository>();
    }
}
