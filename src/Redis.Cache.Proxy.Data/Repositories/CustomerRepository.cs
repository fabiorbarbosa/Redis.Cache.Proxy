using System.Linq.Expressions;
using Redis.Cache.Proxy.Data.Contexts;
using Redis.Cache.Proxy.Data.Entities;
using Redis.Cache.Proxy.Extensions.Attributes;

namespace Redis.Cache.Proxy.Data.Repositories;

[RedisCached(expirationInMinutes: 1)]
internal class CustomerRepository(CacheDbContext db) : ICustomerRepository
{
    private readonly CacheDbContext _db = db;

    public Task<IEnumerable<Customer>> GetAsync(Expression<Func<Customer, bool>> filter)
        => Task.FromResult(_db.Customers.Where(filter).AsEnumerable());
}
