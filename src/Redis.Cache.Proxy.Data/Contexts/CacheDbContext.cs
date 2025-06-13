using Redis.Cache.Proxy.Data.Entities;

namespace Redis.Cache.Proxy.Data.Contexts;

internal class CacheDbContext
{
    private readonly IQueryableAsyncEnumerable<Customer> customers =
        (IQueryableAsyncEnumerable<Customer>)new List<Customer>
        {
            new Customer { Id = 1, Name = "João", City = "São Paulo" },
            new Customer { Id = 2, Name = "Maria", City = "Rio de Janeiro" },
            new Customer { Id = 3, Name = "Ana", City = "São Paulo" },
        }.AsQueryable();

    public IQueryableAsyncEnumerable<Customer> Customers => customers;
}

internal interface IQueryableAsyncEnumerable<out T> : IQueryable<T>, IAsyncEnumerable<T> { }
