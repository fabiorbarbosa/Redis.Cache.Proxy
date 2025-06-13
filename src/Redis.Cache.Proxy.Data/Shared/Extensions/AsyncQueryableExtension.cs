using System.Collections;
using System.Linq.Expressions;

namespace Redis.Cache.Proxy.Data.Shared.Extensions;

internal static class AsyncQueryableExtensions
{
    public static IQueryableAsyncEnumerable<T> ToAsyncQueryable<T>(this IEnumerable<T> source)
        => new AsyncQueryableAdapter<T>(source);

    private sealed class AsyncQueryableAdapter<T>(IEnumerable<T> source)
        : IQueryableAsyncEnumerable<T>
    {
        private readonly IQueryable<T> _queryable = source.AsQueryable();

        public Type ElementType => _queryable.ElementType;
        public Expression Expression => _queryable.Expression;
        public IQueryProvider Provider => _queryable.Provider;

        public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            foreach (var item in _queryable)
            {
                yield return item;
                await Task.Yield();
            }
        }
    }
}
internal interface IQueryableAsyncEnumerable<out T> : IQueryable<T>, IAsyncEnumerable<T> { }
