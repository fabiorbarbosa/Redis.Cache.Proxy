using Redis.Cache.Proxy.Data.Entities;

namespace Redis.Cache.Proxy.Data.Contexts;

internal class CacheDbContext
{
    public FakeDbSet<Customer> Customers { get; } =
        new FakeDbSet<Customer>(
        [
            new() { Id = 1, Name = "João", City = "São Paulo" },
            new() { Id = 2, Name = "Maria", City = "Rio de Janeiro" },
            new() { Id = 3, Name = "Ana", City = "São Paulo" },
        ]);
}