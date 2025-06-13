using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Redis.Cache.Proxy.Infra.Configurations;
using Redis.Cache.Proxy.Application.Customers;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddProblemDetails();

// Add OpenAPI documentation
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(10)));
});
builder.Services.AddOpenApi();

// Register dependencies
builder.Services.AddDependencies();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "Redis Cache Proxy API Cities");
    });
    app.UseReDoc(c =>
    {
        c.DocumentTitle = "Redis Cache Proxy API Cities";
        c.SpecUrl = "/openapi/v1.json";
    });
    app.MapScalarApiReference();
}

// Endpoint with filter by city
app.MapGet("/cities/{city}", async ([FromRoute] string city, ICustomerService customerService) =>
{
    var result = await customerService.GetByCityAsync(city);
    return Results.Ok(new { Total = result.Count(), Cities = result });
})
.WithName("GetClientesByCity")
.WithOpenApi();

// Endpoint to get all clients
app.MapGet("/cities", async (ICustomerService customerService) =>
{
    var result = await customerService.GetAllAsync();
    return Results.Ok(new { Total = result.Count(), Cities = result });
})
.WithName("GetAllClientes")
.WithOpenApi();

await app.RunAsync();