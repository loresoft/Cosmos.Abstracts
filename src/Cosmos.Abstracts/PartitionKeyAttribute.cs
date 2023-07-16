using System;

namespace Cosmos.Abstracts;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PartitionKeyAttribute : Attribute
{

}
