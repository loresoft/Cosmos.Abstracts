using System;

namespace Cosmos.Abstracts.Tests.Models
{
    public class Entry : CosmosEntity
    {
        [PartitionKey]
        public DateTimeOffset EntryDate { get; set; }
    }
}