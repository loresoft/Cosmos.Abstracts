using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Linq;

namespace Cosmos.Abstracts.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IQueryable"/>
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Asynchronously returns the first element of a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}" /> to return the first element of.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the first element of a sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The source sequence is empty.</exception>
        public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using var feedIterator = source.ToFeedIterator();

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);

                return response.Resource.First();
            }

            throw new InvalidOperationException("The source sequence is empty.");
        }

        /// <summary>
        /// Asynchronously returns the first element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}" /> to return the first element of.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the first element of a sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The source sequence is empty.</exception>
        public static async Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await source.Where(predicate).FirstAsync();
        }

        /// <summary>
        /// Asynchronously returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}" /> to return the first element of.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the first element of a sequence, or a default value if the sequence contains no elements.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <c>null</c>.</exception>
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            using var feedIterator = source.ToFeedIterator();

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);

                return response.Resource.FirstOrDefault();
            }

            return default;
        }

        /// <summary>
        /// Asynchronously returns the first element of a sequence that satisfies a specified condition, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}" /> to return the first element of.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the first element of a sequence, or a default value if the sequence contains no elements.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.</exception>
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await source.Where(predicate).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Creates a <see cref="List{TSource}" /> from an <see cref="IQueryable{TSource}" /> by enumerating it asynchronously.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}" /> to create a <see cref="List{TSource}" /> from.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="List{TSource}" /> that contains elements from the input sequence.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source" /> is <c>null</c>.</exception>
        public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var results = new List<TSource>();
            using var feedIterator = source.ToFeedIterator();

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator
                    .ReadNextAsync(cancellationToken)
                    .ConfigureAwait(false);

                results.AddRange(response.Resource);
            }

            return results;
        }
    }
}
