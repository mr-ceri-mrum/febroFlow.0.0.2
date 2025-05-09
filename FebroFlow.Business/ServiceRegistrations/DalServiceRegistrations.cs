using FebroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FebroFlow.Business.ServiceRegistrations;

public static class DalServiceRegistrations
{
    public static IServiceCollection AddDalServices
        (this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Register DAL services
        services.AddScoped<IFlowDal, FlowDal>();
        services.AddScoped<INodeDal, NodeDal>();
        services.AddScoped<IConnectionDal, ConnectionDal>();
        services.AddScoped<IExecutionStateDal, ExecutionStateDal>();
        services.AddScoped<ICredentialDal, CredentialDal>();
        services.AddScoped<IChatMemoryDal, ChatMemoryDal>();
        
        return services;
    }
}