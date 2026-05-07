using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Ambev.DeveloperEvaluation.ORM.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class InfrastructureModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<DefaultContext>());

        // User
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Sale
        builder.Services.AddScoped<ISaleRepository, SaleRepository>();

        // Cart
        builder.Services.AddScoped<ICartRepository, CartRepository>();

        // Product
        builder.Services.AddScoped<IProductRepository, ProductRepository>();

        // Event publisher with Polly retry (logs events via Serilog)
        builder.Services.AddSingleton<IEventPublisher, LoggingEventPublisher>();
    }
}
