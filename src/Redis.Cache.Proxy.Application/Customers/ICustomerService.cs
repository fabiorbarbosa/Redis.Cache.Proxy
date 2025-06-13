using Redis.Cache.Proxy.Application.Models;

namespace Redis.Cache.Proxy.Application.Customers;

public interface ICustomerService
{
    Task<IEnumerable<CustomerModel>> GetAllAsync();
    Task<IEnumerable<CustomerModel>> GetByCityAsync(string city);
}
