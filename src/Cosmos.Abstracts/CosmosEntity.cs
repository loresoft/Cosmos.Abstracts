using System;
using Cosmos.Abstracts.Converters;
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

        /// <summary>
        /// Gets or sets the time to live.
        /// </summary>
        /// <value>
        /// The time to live.
        /// </value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "ttl")]
        public int? TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the etag.
        /// </summary>
        /// <value>
        /// The etag.
        /// </value>
        [JsonProperty("_etag")]
        public string Etag { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        [JsonProperty("_ts")]
        [JsonConverter(typeof(UnixTimeConverter))]
        public DateTimeOffset Timestamp { get; set; }

        /// <inheritdoc/>
        public virtual PartitionKey GetPartitionKey() => new PartitionKey(Id);
    }
}
