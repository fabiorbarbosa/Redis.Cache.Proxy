namespace Redis.Cache.Proxy.Data.Entities;

internal class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}