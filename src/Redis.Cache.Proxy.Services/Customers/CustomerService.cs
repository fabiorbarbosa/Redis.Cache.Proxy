using Redis.Cache.Proxy.Data.Repositories;
using Redis.Cache.Proxy.Services.Models;

namespace Redis.Cache.Proxy.Services.Customers;

internal class CustomerService(ICustomerRepository customerRepository) : ICustomerService
{
    private readonly ICustomerRepository _customerRepository = customerRepository;

    public async Task<IEnumerable<CustomerModel>> GetAllAsync()
    {
        var customers = await _customerRepository.GetAsync(c => true);
        return customers.Select(c => new CustomerModel
        {
            Id = c.Id,
            Name = c.Name,
            City = c.City
        });
    }

    public async Task<IEnumerable<CustomerModel>> GetByCityAsync(string city)
    {
        var customers = await _customerRepository.GetAsync(c => c.City.Equals(city, StringComparison.OrdinalIgnoreCase));
        var customerModel = customers.Select(c => new CustomerModel
        {
            Id = c.Id,
            Name = c.Name,
            City = c.City
        });

        return customerModel;
    }
}
