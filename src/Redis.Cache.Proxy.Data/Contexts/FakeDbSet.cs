using System.Collections;
using System.Linq.Expressions;

namespace Redis.Cache.Proxy.Data.Contexts;

internal class FakeDbSet<T> : IQueryable<T>, IAsyncEnumerable<T> where T : class
{
    private readonly List<T> _data;

    public FakeDbSet(IEnumerable<T> data)
    {
        _data = data.ToList();
        Provider = _data.AsQueryable().Provider;
        Expression = _data.AsQueryable().Expression;
        ElementType = typeof(T);
    }

    public Type ElementType { get; }
    public Expression Expression { get; }
    public IQueryProvider Provider { get; }

    public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new FakeAsyncEnumerator<T>(_data.GetEnumerator());

    public Task<List<T>> ToListAsync() => Task.FromResult(_data.ToList());

    public Task<T[]> ToArrayAsync() => Task.FromResult(_data.ToArray());

    public Task<T?> SingleOrDefaultAsync(Func<T, bool> predicate)
        => Task.FromResult(_data.SingleOrDefault(predicate));

    public Task<T> FirstAsync(Func<T, bool> predicate)
        => Task.FromResult(_data.First(predicate));

    public Task<T?> FirstOrDefaultAsync(Func<T, bool> predicate)
        => Task.FromResult(_data.FirstOrDefault(predicate));
}
