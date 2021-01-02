using System;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Cosmos.Abstracts
{
    /// <summary>
    /// A base class for a Cosmos DB Entity
    /// </summary>
    public abstract class CosmosEntity : ICosmosEntity
    {
        /// <inheritdoc/>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");


        /// <inheritdoc/>
        public virtual PartitionKey GetPartitionKey() => new PartitionKey(Id);
    }

}
