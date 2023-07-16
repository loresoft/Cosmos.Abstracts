using System;
using System.Net.Http;
using System.Threading.Tasks;

using Cosmos.Abstracts.Extensions;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cosmos.Abstracts;

public class CosmosFactory : ICosmosFactory
{
    private readonly ILogger<CosmosFactory> _logger;
    private readonly CosmosRepositoryOptions _repositoryOptions;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly Lazy<CosmosClient> _lazyClient;
    private readonly Lazy<Task<Database>> _lazyDatabase;

    public CosmosFactory(ILogger<CosmosFactory> logger, IOptions<CosmosRepositoryOptions> repositoryOptions, IHttpClientFactory httpClientFactory)
    {
        if (logger == null)
            throw new ArgumentNullException(nameof(logger));

        if (repositoryOptions == null)
            throw new ArgumentNullException(nameof(repositoryOptions));

        if (httpClientFactory == null)
            throw new ArgumentNullException(nameof(httpClientFactory));

        _logger = logger;
        _repositoryOptions = repositoryOptions.Value;
        _httpClientFactory = httpClientFactory;

        if (_repositoryOptions.ConnectionString.IsNullOrEmpty())
            throw new ArgumentException($"The repository option {nameof(CosmosRepositoryOptions.ConnectionString)} is required.", nameof(repositoryOptions));

        if (_repositoryOptions.DatabaseId.IsNullOrEmpty())
            throw new ArgumentException($"The repository option {nameof(CosmosRepositoryOptions.DatabaseId)} is required.", nameof(repositoryOptions));

        _lazyClient = new Lazy<CosmosClient>(InitializeClient);
        _lazyDatabase = new Lazy<Task<Database>>(InitializeDatabase);
    }


    public CosmosClient GetCosmosClient()
    {
        return _lazyClient.Value;
    }

    public Task<Database> GetDatabaseAsync()
    {
        return _lazyDatabase.Value;
    }


    protected virtual async Task<Database> InitializeDatabase()
    {
        try
        {
            var databaseName = _repositoryOptions.DatabaseId;

            _logger.LogDebug("Initializing Cosmos Database '{database}' ...", databaseName);

            var client = GetCosmosClient();
            var databaseResponse = await client
                .CreateDatabaseIfNotExistsAsync(databaseName)
                .ConfigureAwait(false);

            return databaseResponse.Database;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);

            throw;
        }
    }


    protected virtual CosmosClient InitializeClient()
    {
        _logger.LogDebug("Initializing Cosmos Client ...");

        var options = ClientOptions();
        return new CosmosClient(_repositoryOptions.ConnectionString, options);
    }

    protected virtual CosmosClientOptions ClientOptions()
    {
        return new CosmosClientOptions
        {
            HttpClientFactory = _httpClientFactory.CreateClient,
            AllowBulkExecution = _repositoryOptions.AllowBulkExecution,
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                IgnoreNullValues = true
            }
        };
    }
}
