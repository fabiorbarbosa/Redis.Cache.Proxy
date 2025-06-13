using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Redis.Cache.Proxy.Data.Contexts;
using Redis.Cache.Proxy.Data.Entities;
using Redis.Cache.Proxy.CrossCutting.Shared.Attributes;

namespace Redis.Cache.Proxy.Data.Repositories;

[RedisCached(expirationInMinutes: 1)]
internal class CustomerRepository(CacheDbContext db) : ICustomerRepository
{
    private readonly CacheDbContext _db = db;

    public Task<IEnumerable<Customer>> GetAsync(Expression<Func<Customer, bool>> filter)
        => _db.Customers
            .Where(filter)
            .ToListAsync()
            .ContinueWith(t => t.Result.AsEnumerable());
}