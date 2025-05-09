using FluentValidation.AspNetCore;
using FebroFlow.Business.Validator.Flow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FebroFlow.Business.ServiceRegistrations;

public static class FluentValidationServiceRegistrations
{
    public static IServiceCollection AddFluentValidationServices
        (this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddFluentValidation((fv) =>
            fv.RegisterValidatorsFromAssemblyContaining(typeof(FlowCreateCommandValidator)));
        
        return services;
    }
}