using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;

namespace Cosmos.Abstracts.Tests.Models;

[Container("Items", "/ownerId")]
public class Item : ICosmosEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    [PartitionKey]
    [JsonProperty(PropertyName = "ownerId")]
    public string OwnerId { get; set; }

    public PartitionKey GetPartitionKey()
    {
        return new PartitionKey(OwnerId);
    }
}
