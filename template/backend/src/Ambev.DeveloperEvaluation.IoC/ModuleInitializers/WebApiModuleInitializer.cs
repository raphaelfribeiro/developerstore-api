using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers
{
    public class WebApiModuleInitializer : IModuleInitializer
    {
        public void Initialize(WebApplicationBuilder builder)
        {

            builder.Services.AddControllers();
            builder.Services.AddHealthChecks();
        }
    }
}
