using System;

namespace Cosmos.Abstracts;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ContainerAttribute : Attribute
{
    public ContainerAttribute(string containerName, string partitionKeyPath = "/id")
    {
        if (containerName == null)
            throw new ArgumentNullException(nameof(containerName));

        if (partitionKeyPath == null)
            throw new ArgumentNullException(nameof(partitionKeyPath));

        if (!partitionKeyPath.StartsWith("/"))
            throw new ArgumentException($"Partition key path argument must start with '/': {partitionKeyPath}");

        ContainerName = containerName;
        PartitionKeyPath = partitionKeyPath;
    }

    public string ContainerName { get; }

    public string PartitionKeyPath { get; }
}
