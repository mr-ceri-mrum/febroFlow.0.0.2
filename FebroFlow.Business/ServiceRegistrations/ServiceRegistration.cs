using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FebroFlow.Business.ServiceRegistrations;

public static class ServiceRegistration
{
    public static IServiceCollection AddServices
        (this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddMediatrServices(configuration, environment);
        services.AddFluentValidationServices(configuration, environment);
        services.AddCoreServices(configuration, environment);
        services.AddAutoMapServices(configuration, environment);
        services.AddDalServices(configuration, environment);
        services.AddIntegrationServices(configuration, environment);
        
        return services;
    }
}