using System;

namespace Cosmos.Abstracts
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PartitionKeyAttribute : Attribute
    {
        public PartitionKeyAttribute(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!path.StartsWith("/"))
                throw new ArgumentException($"Path argument must start with '/': {path}");

            Path = path;
        }

        public string Path { get; }

    }
}
