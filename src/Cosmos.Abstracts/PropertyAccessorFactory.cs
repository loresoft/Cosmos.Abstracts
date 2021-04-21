using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

            return CreateGet(property);
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
            var names = new[] { "Id", "Key", type.Name + "Id" };

            var property = type
                .GetProperties()
                .FirstOrDefault(p => names.Contains(p.Name));

            if (property == null)
                return null;

            return CreateGet(property);
        }


        private static Func<object, string> CreateGet(PropertyInfo propertyInfo)
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
            var valueCast = propertyInfo.PropertyType.IsValueType
                    ? Expression.Convert(value, typeof(string))
                    : Expression.TypeAs(value, typeof(string));

            var lambda = Expression.Lambda<Func<object, string>>(valueCast, instance);
            return lambda.Compile();
        }
    }
}
