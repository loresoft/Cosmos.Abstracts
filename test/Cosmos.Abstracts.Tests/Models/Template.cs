using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Cosmos.Abstracts.Tests.Models
{
    [Container(nameof(Template), "/ownerId")]
    public class Template : CosmosEntity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        [PartitionKey]
        [JsonProperty(PropertyName = "ownerId")]
        public string OwnerId { get; set; }

        public override PartitionKey GetPartitionKey()
        {
            return new PartitionKey(OwnerId);
        }
    }
}
