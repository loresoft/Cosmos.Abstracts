using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Abstracts.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Cosmos.Abstracts
{

    /// <summary>
    /// A repository pattern implementation for Cosmos DB data operations.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class CosmosRepository<TEntity> : ICosmosRepository<TEntity>
    {
        private readonly Lazy<Task<Container>> _lazyContainer;

        private readonly Lazy<ContainerAttribute> _containerAttribute;
        private readonly Lazy<Func<object, string>> _partitionKeyAccessor;
        private readonly Lazy<Func<object, string>> _primaryKeyAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosRepository{TEntity}"/> class.
        /// </summary>
        /// <param name="logFactory">The log factory.</param>
        /// <param name="repositoryOptions">The repository options.</param>
        /// <param name="databaseFactory">The database factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="logFactory"/> or <paramref name="repositoryOptions"/> or <paramref name="databaseFactory"/> is null
        /// </exception>
        public CosmosRepository(
            ILoggerFactory logFactory,
            IOptions<CosmosRepositoryOptions> repositoryOptions,
            ICosmosFactory databaseFactory
        )
        {
            if (logFactory == null)
                throw new ArgumentNullException(nameof(logFactory));

            if (repositoryOptions == null)
                throw new ArgumentNullException(nameof(repositoryOptions));

            if (databaseFactory == null)
                throw new ArgumentNullException(nameof(databaseFactory));


            Logger = logFactory.CreateLogger(GetType());

            Options = repositoryOptions.Value;
            Factory = databaseFactory;

            _lazyContainer = new Lazy<Task<Container>>(InitializeContainer);

            _containerAttribute = new Lazy<ContainerAttribute>(GetContainerAttribute);
            _partitionKeyAccessor = new Lazy<Func<object, string>>(PropertyAccessorFactory.CreatePartitionKeyAccessor<TEntity>);
            _primaryKeyAccessor = new Lazy<Func<object, string>>(PropertyAccessorFactory.CreatePrimaryKeyAccessor<TEntity>);
        }

        /// <inheritdoc/>
        public Task<Container> GetContainerAsync()
        {
            return _lazyContainer.Value;
        }


        /// <inheritdoc/>
        public async Task<IOrderedQueryable<TEntity>> GetQueryableAsync(bool allowSynchronousQueryExecution = false, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            var container = await GetContainerAsync().ConfigureAwait(false);

            return container.GetItemLinqQueryable<TEntity>(allowSynchronousQueryExecution, continuationToken, requestOptions);
        }


        /// <inheritdoc/>
        public async Task<TEntity> FindAsync(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default)
        {
            if (id.IsNullOrEmpty())
                return default;

            var container = await GetContainerAsync().ConfigureAwait(false);

            if (partitionKey == default)
                partitionKey = new PartitionKey(id);

            try
            {
                var response = await container
                    .ReadItemAsync<TEntity>(id, partitionKey, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                LogResponse(response);

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<TEntity> FindAsync(string id, string partitionKey = null, CancellationToken cancellationToken = default)
        {
            if (id.IsNullOrEmpty())
                return default;

            var key = partitionKey.HasValue() ? new PartitionKey(partitionKey) : default;

            return await FindAsync(id, key, cancellationToken).ConfigureAwait(false);
        }


        /// <inheritdoc/>
        public async Task<IReadOnlyList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            var results = new List<TEntity>();

            var container = await GetContainerAsync().ConfigureAwait(false);

            var query = container.GetItemLinqQueryable<TEntity>();

            using var feedIterator = query.Where(criteria).ToFeedIterator();

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);

                LogResponse(response);

                results.AddRange(response.Resource);
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<TEntity>> FindAllAsync(QueryDefinition queryDefinition, CancellationToken cancellationToken = default)
        {
            if (queryDefinition == null)
                throw new ArgumentNullException(nameof(queryDefinition));

            var results = new List<TEntity>();

            var container = await GetContainerAsync().ConfigureAwait(false);
            var options = QueryOptions();

            using var feedIterator = container.GetItemQueryIterator<TEntity>(queryDefinition, requestOptions: options);

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);

                LogResponse(response);

                results.AddRange(response.Resource);
            }

            return results;
        }


        /// <inheritdoc/>
        public async Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default)
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            var container = await GetContainerAsync().ConfigureAwait(false);

            var options = QueryOptions();
            options.MaxItemCount = 1;

            var query = container.GetItemLinqQueryable<TEntity>(requestOptions: options);

            using var feedIterator = query.Where(criteria).ToFeedIterator();

            if (feedIterator.HasMoreResults)
            {
                var response = await feedIterator
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);

                LogResponse(response);

                return response.Resource.FirstOrDefault();
            }

            return default;
        }

        /// <inheritdoc/>
        public async Task<TEntity> FindOneAsync(QueryDefinition queryDefinition, CancellationToken cancellationToken = default)
        {
            if (queryDefinition == null)
                throw new ArgumentNullException(nameof(queryDefinition));

            var container = await GetContainerAsync().ConfigureAwait(false);

            var options = QueryOptions();
            options.MaxItemCount = 1;

            using var feedIterator = container.GetItemQueryIterator<TEntity>(queryDefinition, requestOptions: options);

            if (feedIterator.HasMoreResults)
            {
                var response = await feedIterator
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);

                LogResponse(response);

                return response.Resource.FirstOrDefault();
            }

            return default;
        }


        /// <inheritdoc/>
        public async Task<TEntity> SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            BeforeSave(entity);

            var partitionKey = GetPartitionKey(entity);
            var options = CreateItemOptions();

            var container = await GetContainerAsync().ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(entity, partitionKey, options, cancellationToken)
                .ConfigureAwait(false);

            LogResponse(response);

            var result = options.EnableContentResponseOnWrite == true
                ? response.Resource
                : entity;

            AfterSave(result);

            return result;
        }

        /// <inheritdoc/>
        public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            BeforeSave(entity);

            var partitionKey = GetPartitionKey(entity);
            var options = CreateItemOptions();

            var container = await GetContainerAsync().ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(entity, partitionKey, options, cancellationToken)
                .ConfigureAwait(false);

            LogResponse(response);

            var result = options.EnableContentResponseOnWrite == true
                ? response.Resource
                : entity;

            AfterSave(result);

            return result;
        }

        /// <inheritdoc/>
        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            BeforeSave(entity);

            var partitionKey = GetPartitionKey(entity);
            var options = UpdateItemOptions();

            var container = await GetContainerAsync().ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(entity, partitionKey, options, cancellationToken)
                .ConfigureAwait(false);

            LogResponse(response);

            var result = options.EnableContentResponseOnWrite == true
                ? response.Resource
                : entity;

            AfterSave(result);

            return result;
        }


        /// <inheritdoc/>
        public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var partitionKey = GetPartitionKey(entity);
            var entityKey = GetEntityKey(entity);

            await DeleteAsync(entityKey, partitionKey, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default)
        {
            if (id.IsNullOrEmpty())
                return;

            var container = await GetContainerAsync().ConfigureAwait(false);
            var options = DeleteItemOptions();

            if (partitionKey == default)
                partitionKey = new PartitionKey(id);

            var response = await container
                .DeleteItemAsync<TEntity>(id, partitionKey, options, cancellationToken)
                .ConfigureAwait(false);

            LogResponse(response);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string id, string partitionKey = null, CancellationToken cancellationToken = default)
        {
            if (id.IsNullOrEmpty())
                return;

            var key = partitionKey.HasValue() ? new PartitionKey(partitionKey) : default;

            await DeleteAsync(id, key, cancellationToken).ConfigureAwait(false);
        }


        /// <summary>
        /// Gets the diagnostics logger instance.
        /// </summary>
        /// <value>
        /// The diagnostics logger instance.
        /// </value>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the Cosmos DB repository options.
        /// </summary>
        /// <value>
        /// The Cosmos DB repository options.
        /// </value>
        protected CosmosRepositoryOptions Options { get; }

        /// <summary>
        /// Gets the Cosmos DB factory methods.
        /// </summary>
        /// <value>
        /// The Cosmos DB factory methods.
        /// </value>
        protected ICosmosFactory Factory { get; }


        /// <summary>
        /// Called before a saving the specified <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity being saved.</param>
        protected virtual void BeforeSave(TEntity entity)
        {
            var cosmosEntity = entity as ICosmosEntity;
            if (cosmosEntity == null)
                return;

            if (cosmosEntity.Id.IsNullOrEmpty())
                cosmosEntity.Id = ObjectId.GenerateNewId().ToString();
        }

        /// <summary>
        /// Called after a saving the specified <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity being saved.</param>
        protected virtual void AfterSave(TEntity entity)
        {
        }


        /// <summary>
        /// Gets the name for this repository container.
        /// </summary>
        /// <returns>The name for this repository container.</returns>
        /// <remarks>
        /// This is called with creating the Comos DB container for this repository.
        /// </remarks>
        protected virtual string GetContainerName()
        {
            var attribute = _containerAttribute.Value;
            return attribute != null
                ? attribute.ContainerName
                : typeof(TEntity).Name;
        }

        /// <summary>
        /// Gets the partition key path for this repository container. The path must start '/' char.
        /// </summary>
        /// <returns>The partition key path this repository container.  </returns>
        /// <remarks>
        /// This is called with creating the Comos DB container for this repository.
        /// </remarks>
        protected virtual string GetPartitionKeyPath()
        {
            var attribute = _containerAttribute.Value;
            if (attribute != null)
                return attribute.PartitionKeyPath;

            var type = typeof(TEntity);
            var attributeType = typeof(PartitionKeyAttribute);

            var property = type
                .GetProperties()
                .FirstOrDefault(p => Attribute.IsDefined(p, attributeType));

            if (property == null)
                return "/id";

            var jsonAttribute = Attribute.GetCustomAttribute(property, typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;
            if (jsonAttribute != null && jsonAttribute.PropertyName.HasValue())
                return "/" + jsonAttribute.PropertyName;

            var cosmosClient = Factory.GetCosmosClient();
            var namingPolicy = cosmosClient.ClientOptions?.SerializerOptions?.PropertyNamingPolicy;

            return "/" + (namingPolicy == CosmosPropertyNamingPolicy.CamelCase ? property.Name.ToCamelCase() : property.Name);
        }

        /// <summary>
        /// Gets the partition key for the specified <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity to get the partition key from.</param>
        /// <returns>The <see cref="PartitionKey"/> for this entity.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> is null</exception>
        /// <remarks>
        /// First, looks for the <see cref="PartitionKeyAttribute"/> to determine which property is the partition key.
        /// Otherwise, uses the entity key for the partition key.
        /// </remarks>
        protected virtual PartitionKey GetPartitionKey(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity is ICosmosEntity cosmosEntity)
                return cosmosEntity.GetPartitionKey();

            var accessor = _partitionKeyAccessor.Value;
            if (accessor != null)
            {
                var partitionValue = accessor(entity);
                return new PartitionKey(partitionValue);
            }

            var key = GetEntityKey(entity);
            return new PartitionKey(key);
        }

        /// <summary>
        /// Gets the entity key for the specified <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">The entity to get the key from.</param>
        /// <returns>The key value for this entity.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="entity"/> is null</exception>
        /// <exception cref="NotImplementedException">Either override GetEntityKey() or add a property named Id to the entity type.</exception>
        protected virtual string GetEntityKey(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity is ICosmosEntity cosmosEntity)
                return cosmosEntity.Id;

            var accessor = _primaryKeyAccessor.Value;
            if (accessor == null)
                throw new NotImplementedException("Either override GetEntityKey() or add a property named Id to the entity type.");

            return accessor(entity);
        }


        /// <summary>
        /// Initializes the container on first use. Override to customize how the <see cref="Container"/> is created in Cosmos DB.
        /// </summary>
        /// <returns>A <see cref="Container"/> instance.</returns>
        protected virtual async Task<Container> InitializeContainer()
        {
            try
            {
                var database = await Factory
                    .GetDatabaseAsync()
                    .ConfigureAwait(false);

                var containerProperties = ContainerProperties();

                Logger.LogDebug("Initializing Cosmos Container '{container}' ...", containerProperties.Id);

                var containerResponse = await database
                    .CreateContainerIfNotExistsAsync(containerProperties)
                    .ConfigureAwait(false);

                return containerResponse.Container;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);

                throw;
            }
        }


        /// <summary>
        /// Logs the service responses.
        /// </summary>
        /// <typeparam name="T">The return type from the service response.</typeparam>
        /// <param name="response">The response instance.</param>
        /// <param name="memberName">Name of the calling member.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceLineNumber">The source line number.</param>
        protected virtual void LogResponse<T>(Response<T> response, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (response == null)
                return;

            if (!Logger.IsEnabled(LogLevel.Debug))
                return;

            Logger.LogDebug(
                "Response from '{memberName}'; Status: '{statusCode}'; Charge: {charge} RU; Elapsed: {elapsedTime} ms; File: '{fileName}' ({lineNumber})",
                memberName,
                response.StatusCode,
                response.RequestCharge,
                response.Diagnostics?.GetClientElapsedTime().TotalMilliseconds ?? 0,
                Path.GetFileName(sourceFilePath),
                sourceLineNumber
            );
        }


        /// <summary>
        /// The default <see cref="ItemRequestOptions"/> for create operations
        /// </summary>
        /// <returns>The <see cref="ItemRequestOptions"/> instance.</returns>
        protected virtual ItemRequestOptions CreateItemOptions()
        {
            return new ItemRequestOptions
            {
                EnableContentResponseOnWrite = !Options.OptimizeBandwidth
            };
        }

        /// <summary>
        /// The default <see cref="ItemRequestOptions"/> for update operations
        /// </summary>
        /// <returns>The <see cref="ItemRequestOptions"/> instance.</returns>
        protected virtual ItemRequestOptions UpdateItemOptions()
        {
            return new ItemRequestOptions
            {
                EnableContentResponseOnWrite = !Options.OptimizeBandwidth
            };
        }

        /// <summary>
        /// The default <see cref="ItemRequestOptions"/> for delete operations
        /// </summary>
        /// <returns>The <see cref="ItemRequestOptions"/> instance.</returns>
        protected virtual ItemRequestOptions DeleteItemOptions()
        {
            return new ItemRequestOptions();
        }

        /// <summary>
        /// The default <see cref="QueryRequestOptions"/> for query operations
        /// </summary>
        /// <returns>The <see cref="QueryRequestOptions"/> instance.</returns>
        protected virtual QueryRequestOptions QueryOptions()
        {
            return new QueryRequestOptions();
        }

        /// <summary>
        /// The default <see cref="ContainerProperties"/> to use when creating a <see cref="Container"/> Cosmos DB.
        /// </summary>
        /// <returns>The <see cref="ContainerProperties"/> instance.</returns>
        protected virtual ContainerProperties ContainerProperties()
        {
            return new ContainerProperties
            {
                Id = GetContainerName(),
                PartitionKeyPath = GetPartitionKeyPath()
            };
        }


        private ContainerAttribute GetContainerAttribute()
        {
            var type = typeof(TEntity);
            var attributeType = typeof(ContainerAttribute);

            return Attribute.GetCustomAttribute(type, attributeType) as ContainerAttribute;
        }
    }

}
