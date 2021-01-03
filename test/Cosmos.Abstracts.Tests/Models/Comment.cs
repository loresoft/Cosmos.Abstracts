using Microsoft.Azure.Cosmos;

namespace Cosmos.Abstracts.Tests.Models
{
    public class Comment
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [PartitionKey]
        public string OwnerId { get; set; }
    }
}
