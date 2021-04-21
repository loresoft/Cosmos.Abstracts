using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Cosmos.Abstracts
{
    /// <summary>
    /// Property accessor factory methods
    /// </summary>
    public class PropertyAccessorFactory
    {
        /// <summary>
        /// Creates the partition key accessor.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        public static Func<object, string> CreatePartitionKeyAccessor<TEntity>()
        {
            var type = typeof(TEntity);
            return CreatePartitionKeyAccessor(type);
        }

        /// <summary>
        /// Creates the partition key accessor.
        /// </summary>
        /// <param name="type">The type of the entity.</param>
        /// <returns></returns>
        public static Func<object, string> CreatePartitionKeyAccessor(Type type)
        {
            var attributeType = typeof(PartitionKeyAttribute);

            var property = type
                .GetProperties()
                .FirstOrDefault(p => Attribute.IsDefined(p, attributeType));

            if (property == null)
                return null;

            var getMethod = property.GetGetMethod(false);
            if (getMethod == null || property.GetIndexParameters().Length != 0)
                return null;

            var instance = Expression.Parameter(type, "instance");
            var value = Expression.Call(instance, getMethod);

            var expression = Expression.Lambda<Func<object, string>>(
                property.PropertyType.IsValueType
                    ? Expression.Convert(value, typeof(string))
                    : Expression.TypeAs(value, typeof(string)),
                instance
            );

            return expression.Compile();
        }

        /// <summary>
        /// Creates the primary key accessor.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        public static Func<object, string> CreatePrimaryKeyAccessor<TEntity>()
        {
            var type = typeof(TEntity);
            return CreatePrimaryKeyAccessor(type);
        }

        /// <summary>
        /// Creates the primary key accessor.
        /// </summary>
        /// <param name="type">The type of the entity.</param>
        /// <returns></returns>
        public static Func<object, string> CreatePrimaryKeyAccessor(Type type)
        {
            var names = new[] {"Id", "Key", type.Name + "Id"};

            var property = type
                .GetProperties()
                .FirstOrDefault(p => names.Contains(p.Name));

            if (property == null)
                return null;

            var getMethod = property.GetGetMethod(false);
            if (getMethod == null || property.GetIndexParameters().Length != 0)
                return null;

            var instance = Expression.Parameter(type, "instance");
            var value = Expression.Call(instance, getMethod);

            var expression = Expression.Lambda<Func<object, string>>(
                property.PropertyType.IsValueType
                    ? Expression.Convert(value, typeof(string))
                    : Expression.TypeAs(value, typeof(string)),
                instance
            );

            return expression.Compile();
        }
    }
}
