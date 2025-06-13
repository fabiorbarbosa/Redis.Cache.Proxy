using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Redis.Cache.Proxy.Infra.Configurations;
using Redis.Cache.Proxy.Services.Customers;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Redis Cache Proxy API", Version = "v1" });
});

// Registra as dependências
builder.Services.AddDependencies();

var app = builder.Build();
// Configurações do swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Redis Cache Proxy API V1");
    c.RoutePrefix = string.Empty; // Define a raiz do Swagger
});

// Endpoint com filtro por cidade
app.MapGet("/clientes/{cidade}", async ([FromRoute] string cidade, ICustomerService customerService) =>
{
    var result = await customerService.GetByCityAsync(cidade);
    return Results.Ok(new { Total = result.Count(), Clientes = result });
});

// Endpoint para obter todos os clientes
app.MapGet("/clientes", async (ICustomerService customerService) =>
{
    var result = await customerService.GetAllAsync();
    return Results.Ok(new { Total = result.Count(), Clientes = result });
});

await app.RunAsync();