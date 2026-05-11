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

        // Unit of Work
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // User
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Sale
        builder.Services.AddScoped<ISaleRepository, SaleRepository>();

        // Cart
        builder.Services.AddScoped<ICartRepository, CartRepository>();

        // Product
        builder.Services.AddScoped<IProductRepository, ProductRepository>();

        // Set up Rebus service bus (RabbitMQ when configured, InMemory otherwise).
        builder.AddRebusMessaging();

        // Register RebusEventPublisher as a concrete singleton so MongoEventPublisher can wrap it.
        builder.Services.AddSingleton<RebusEventPublisher>();

        // Register IEventPublisher: MongoDB-backed decorator when a connection string is present,
        // Rebus-only fallback otherwise.
        if (!builder.AddMongoEventStore())
            builder.Services.AddSingleton<IEventPublisher>(sp =>
                sp.GetRequiredService<RebusEventPublisher>());
    }
}
