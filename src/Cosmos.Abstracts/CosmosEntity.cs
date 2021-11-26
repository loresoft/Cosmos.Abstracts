using System;
using System.Reflection;
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
        private readonly Lazy<Func<object, string>> _partitionKeyAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosEntity"/> class.
        /// </summary>
        protected CosmosEntity()
        {
            _partitionKeyAccessor = new Lazy<Func<object, string>>(() => PropertyAccessorFactory.CreatePartitionKeyAccessor(GetType()));
        }

        /// <inheritdoc/>
        [JsonProperty("id")]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        /// <summary>
        /// Gets or sets the time to live.
        /// </summary>
        /// <value>
        /// The time to live.
        /// </value>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "ttl")]
        public int? TimeToLive { get; set; }

        /// <summary>
        /// Gets or sets the system generated property that specifies the resource etag required for optimistic concurrency control.
        /// </summary>
        /// <value>
        /// The system generated property that specifies the resource etag required for optimistic concurrency control.
        /// </value>
        [JsonProperty("_etag")]
        public string Etag { get; set; }

        /// <summary>
        /// Gets or sets the system generated last updated timestamp of the resource
        /// </summary>
        /// <value>
        /// The system generated last updated timestamp of the resource
        /// </value>
        [JsonProperty("_ts")]
        public int Timestamp { get; set; }

        /// <inheritdoc/>
        public virtual PartitionKey GetPartitionKey()
        {
            var accessor = _partitionKeyAccessor.Value;
            if (accessor == null)
                return new PartitionKey(Id);

            var partitionValue = accessor(this);
            return new PartitionKey(partitionValue);
        }
    }
}
