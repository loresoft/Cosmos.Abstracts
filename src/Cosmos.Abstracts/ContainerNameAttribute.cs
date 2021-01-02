using System;

namespace Cosmos.Abstracts
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ContainerNameAttribute : Attribute
    {
        public ContainerNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
