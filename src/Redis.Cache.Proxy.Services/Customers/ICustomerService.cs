using Redis.Cache.Proxy.Services.Models;

namespace Redis.Cache.Proxy.Services.Customers;

public interface ICustomerService
{
    Task<IEnumerable<CustomerModel>> GetAllAsync();
    Task<IEnumerable<CustomerModel>> GetByCityAsync(string city);
}
