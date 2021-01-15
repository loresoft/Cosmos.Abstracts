using System;
using Cosmos.Abstracts;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosRepository(this IServiceCollection services, Action<CosmosRepositoryOptions> setupAction = default)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services), "A service collection is required.");

            services.AddOptions<CosmosRepositoryOptions>()
                    .Configure<IConfiguration>((settings, configuration) => configuration.GetSection(CosmosRepositoryOptions.ConfigurationName).Bind(settings));

            services.AddLogging()
                    .AddHttpClient()
                    .AddSingleton<ICosmosFactory, CosmosFactory>()
                    .AddSingleton(typeof(ICosmosRepository<>), typeof(CosmosRepository<>));

            if (setupAction != default)
                services.PostConfigure(setupAction);

            return services;
        }

    }

}
