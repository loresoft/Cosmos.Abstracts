using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Cosmos.Abstracts
{
    /// <summary>
    /// Property accessor factory methods
    /// </summary>
    public static class PropertyAccessorFactory
    {
        private static readonly ConcurrentDictionary<Type, Func<object, string>> _partitionKeyCache = new ConcurrentDictionary<Type, Func<object, string>>();
        private static readonly ConcurrentDictionary<Type, Func<object, string>> _primaryKeyCache = new ConcurrentDictionary<Type, Func<object, string>>();

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
            return _partitionKeyCache.GetOrAdd(type, innerType =>
            {
                var attributeType = typeof(PartitionKeyAttribute);

                var property = innerType
                    .GetProperties()
                    .FirstOrDefault(p => Attribute.IsDefined(p, attributeType));

                if (property == null)
                    return null;

                return CreateGetDelegate(property);
            });
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
            return _primaryKeyCache.GetOrAdd(type, innerType =>
            {
                var names = new[] { "Id", "Key", type.Name + "Id" };

                var property = type
                    .GetProperties()
                    .FirstOrDefault(p => names.Contains(p.Name));

                if (property == null)
                    return null;

                return CreateGetDelegate(property);
            });
        }


        private static Func<object, string> CreateGetDelegate(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (!propertyInfo.CanRead)
                return null;

            var getMethod = propertyInfo.GetGetMethod(true);
            if (getMethod == null || propertyInfo.GetIndexParameters().Length != 0)
                return null;

            var instance = Expression.Parameter(typeof(object), "instance");
            var declaringType = propertyInfo.DeclaringType;
            if (declaringType == null)
                throw new ArgumentException("The specified PropertyInfo DeclaringType is null.", nameof(propertyInfo));

            UnaryExpression instanceCast;
            if (getMethod.IsStatic)
                instanceCast = null;
            else if (declaringType.IsValueType)
                instanceCast = Expression.Convert(instance, declaringType);
            else
                instanceCast = Expression.TypeAs(instance, declaringType);

            var value = Expression.Call(instanceCast, getMethod);

            if (propertyInfo.PropertyType == typeof(string))
                return Expression.Lambda<Func<object, string>>(value, instance).Compile();

            var toString = Expression.Call(value, "ToString", Type.EmptyTypes);
            return Expression.Lambda<Func<object, string>>(toString, instance).Compile();
        }
    }
}
