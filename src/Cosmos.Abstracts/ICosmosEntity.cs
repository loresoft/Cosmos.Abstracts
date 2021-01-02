using Microsoft.Azure.Cosmos;

namespace Cosmos.Abstracts
{
    /// <summary>
    /// An <c>interface</c> for a Cosmos DB Entity
    /// </summary>
    public interface ICosmosEntity
    {
        /// <summary>
        /// Gets or sets the identifier for the entity.
        /// </summary>
        /// <value>
        /// The identifier for the entity.
        /// </value>
        string Id { get; set; }

        /// <summary>
        /// Gets the partition key for this entity.
        /// </summary>
        /// <returns>The <see cref="PartitionKey"/> for this entity.</returns>
        PartitionKey GetPartitionKey();
    }

}
