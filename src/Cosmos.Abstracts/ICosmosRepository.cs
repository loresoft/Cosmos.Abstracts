using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;

namespace Cosmos.Abstracts
{
    /// <summary>
    /// An <c>interface</c> for common Cosmos DB data operations.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface ICosmosRepository<TEntity>
    {
        /// <summary>
        /// Gets the Cosmos DB <see cref="Container"/> to use for data operations.
        /// </summary>
        /// <returns>A <see cref="Container"/> instance.</returns>
        Task<Container> GetContainerAsync();

        /// <summary>
        /// Creates a LINQ <see cref="IOrderedQueryable{TEntity}" /> for the <typeparamref name="TEntity" /> items.
        /// </summary>
        /// <param name="allowSynchronousQueryExecution">(Optional) The option which allows the query to be executed synchronously.</param>
        /// <param name="continuationToken">(Optional) The continuation token in the Azure Cosmos DB service.</param>
        /// <param name="requestOptions">(Optional) The options for the item query request.</param>
        /// <returns>
        /// An <see cref="IOrderedQueryable{TEntity}" /> that can evaluate the query.
        /// </returns>
        Task<IOrderedQueryable<TEntity>> GetQueryableAsync(bool allowSynchronousQueryExecution = false, string continuationToken = null, QueryRequestOptions requestOptions = null);

        /// <summary>
        /// Find an entity with the specified <paramref name="id" /> and <paramref name="partitionKey" />.
        /// </summary>
        /// <param name="id">The entity identifier to find.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of <typeparamref name="TEntity"/> that has the specified identifier if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="id"/> is <see langword="null" />.</exception>
        Task<TEntity> FindAsync(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find an entity with the specified <paramref name="id" /> and <paramref name="partitionKey" />.
        /// </summary>
        /// <param name="id">The entity identifier to find.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of <typeparamref name="TEntity"/> that has the specified identifier if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="id"/> is <see langword="null" />.</exception>
        Task<TEntity> FindAsync(string id, string partitionKey = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find all entities using the specified <paramref name="criteria"/> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A list of <typeparamref name="TEntity"/> instances that matches the criteria if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="criteria"/> is null</exception>
        Task<IReadOnlyList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find all entities using the specified <paramref name="queryDefinition"/>.
        /// </summary>
        /// <param name="queryDefinition">The query definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A list of <typeparamref name="TEntity"/> instances that matches the query if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="queryDefinition"/> is null</exception>
        Task<IReadOnlyList<TEntity>> FindAllAsync(QueryDefinition queryDefinition, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find the first entity using the specified <paramref name="criteria"/> expression.
        /// </summary>
        /// <param name="criteria">The criteria expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of <typeparamref name="TEntity"/> that matches the criteria if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="criteria"/> is null</exception>
        Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find the first entity using the specified <paramref name="queryDefinition"/>.
        /// </summary>
        /// <param name="queryDefinition">The query definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An instance of <typeparamref name="TEntity"/> that matches the query if found, otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="queryDefinition"/> is null</exception>
        Task<TEntity> FindOneAsync(QueryDefinition queryDefinition, CancellationToken cancellationToken = default);


        /// <summary>
        /// Saves the specified <paramref name="entity" /> in the underlying data store by inserting if doesn't exist, or updating if it does.
        /// </summary>
        /// <param name="entity">The entity to be saved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The <typeparamref name="TEntity"/> that was saved.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="entity"/> is null</exception>
        Task<TEntity> SaveAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts the specified <paramref name="entity" /> to the underlying data store.
        /// </summary>
        /// <param name="entity">The entity to be inserted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The <typeparamref name="TEntity"/> that was inserted.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="entity"/> is null</exception>
        Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the specified <paramref name="entity" /> in the underlying data store.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The <typeparamref name="TEntity"/> that was updated.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="entity"/> is null</exception>
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);


        /// <summary>
        /// Deletes the specified <paramref name="entity" /> from the underlying data store.
        /// </summary>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="entity"/> is null</exception>
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity with the specified <paramref name="id" /> and <paramref name="partitionKey" /> from the underlying data store.
        /// </summary>
        /// <param name="id">The entity identifier to delete.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="id"/> is <see langword="null" />.</exception>
        Task DeleteAsync(string id, PartitionKey partitionKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity with the specified <paramref name="id" /> and <paramref name="partitionKey" /> from the underlying data store.
        /// </summary>
        /// <param name="id">The entity identifier to delete.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="id"/> is <see langword="null" />.</exception>
        Task DeleteAsync(string id, string partitionKey = default, CancellationToken cancellationToken = default);


        /// <summary>
        /// Patch an entity with the specified <paramref name="id" /> and <paramref name="partitionKey" /> from the underlying data store.
        /// </summary>
        /// <param name="id">The entity identifier to patch.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="patchOperations">The patch operations.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TEntity> PatchAsync(string id, PartitionKey partitionKey, IReadOnlyList<PatchOperation> patchOperations, CancellationToken cancellationToken = default);

        /// <summary>
        /// Patch an entity with the specified <paramref name="id" /> and <paramref name="partitionKey" /> from the underlying data store.
        /// </summary>
        /// <param name="id">The entity identifier to patch.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="patchOperations">The patch operations.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TEntity> PatchAsync(string id, string partitionKey, IReadOnlyList<PatchOperation> patchOperations, CancellationToken cancellationToken = default);
    }

}
