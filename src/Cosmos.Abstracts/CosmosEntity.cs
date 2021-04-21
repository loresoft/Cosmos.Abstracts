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
        [JsonProperty(PropertyName = "id")]
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
        /// Gets or sets the etag.
        /// </summary>
        /// <value>
        /// The etag.
        /// </value>
        [JsonProperty("_etag")]
        public string Etag { get; set; }

        /// <summary>
        /// Gets or sets the date and time, in UTC, this entity was created.
        /// </summary>
        /// <value>
        /// The date and time this entity was created.
        /// </value>
        [JsonProperty(PropertyName = "created")]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the date and time this entity was updated.
        /// </summary>
        /// <value>
        /// The date and time this entity was updated.
        /// </value>
        [JsonProperty("_ts")]
        [JsonConverter(typeof(UnixTimeConverter))]
        public DateTimeOffset Updated { get; set; }

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
