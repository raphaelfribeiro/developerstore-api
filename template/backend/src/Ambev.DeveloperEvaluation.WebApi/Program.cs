using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Common.HealthChecks;
using Ambev.DeveloperEvaluation.Common.Logging;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

namespace Ambev.DeveloperEvaluation.WebApi;

public partial class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.AddDefaultLogging();

            // Configure Kestrel to listen on port 8080
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8080);
            });

            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

            builder.Services.AddEndpointsApiExplorer();

            builder.AddBasicHealthChecks();

            // Swagger with XML documentation support
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "DeveloperStore API",
                    Version = "v1",
                    Description = "Sales management API with Cart and Product support. " +
                                  "Business rules: 4-9 items → 10% discount | 10-20 items → 20% discount | max 20 items per product."
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
                )
            );

            builder.RegisterDependencies();

            // Add JWT Authentication
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // AutoMapper configuration can be added here if needed, but for now, we are just scanning the assemblies for profiles.
            builder.Services.AddAutoMapper(config => { }, typeof(Program).Assembly, typeof(ApplicationLayer).Assembly);

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    typeof(ApplicationLayer).Assembly,
                    typeof(Program).Assembly
                );
            });

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            Log.Information("Application started. Building app...");

            var app = builder.Build();

            Log.Information("App built. Starting database migration process...");
            app.UseMiddleware<ValidationExceptionMiddleware>();

            // Swagger always enabled (useful for evaluators)
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DeveloperStore API v1");
                c.RoutePrefix = string.Empty; // Swagger at root
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseBasicHealthChecks();
            app.MapControllers();

            // Apply pending migrations at startup
            try
            {
                Log.Information("Starting database migration process..."); 
                
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
                    Log.Information("Applying database migrations...");
                    db.Database.Migrate();
                    Log.Information("Database migrations applied successfully.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to apply database migrations");
                throw; // Re-throw to prevent app from starting
            }

            Log.Information("Starting web server...");
            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Web server terminated unexpectedly");
                throw;
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}