using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Redis.Cache.Proxy.Infra.Configurations;
using Redis.Cache.Proxy.Application.Customers;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Redis Cache Proxy API", Version = "v1" });
});

// Register dependencies
builder.Services.AddDependencies();

var app = builder.Build();
// Swagger settings
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Redis Cache Proxy API V1");
    c.RoutePrefix = "docs";
});

// Endpoint with filter by city
app.MapGet("/cities/{city}", async ([FromRoute] string city, ICustomerService customerService) =>
{
    var result = await customerService.GetByCityAsync(city);
    return Results.Ok(new { Total = result.Count(), Cities = result });
});

// Endpoint to get all clients
app.MapGet("/cities", async (ICustomerService customerService) =>
{
    var result = await customerService.GetAllAsync();
    return Results.Ok(new { Total = result.Count(), Cities = result });
});

await app.RunAsync();