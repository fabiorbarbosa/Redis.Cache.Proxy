

using System.Linq.Expressions;
using Redis.Cache.Proxy.Data.Entities;

namespace Redis.Cache.Proxy.Data.Repositories;

internal interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAsync(Expression<Func<Customer, bool>> filter);
}