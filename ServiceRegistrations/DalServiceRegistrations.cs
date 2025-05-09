using febroFlow.DataAccess.DataAccess;
using febroFlow.DataAccess.DataAccess.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace febroFlow.Business.ServiceRegistrations;

public static class DalServiceRegistrations
{
    public static IServiceCollection AddDalServices
        (this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Register Flow DAL
        services.AddScoped<IFlowDal, FlowDal>();
        
        // Register Node DAL
        services.AddScoped<INodeDal, NodeDal>();
        
        // Register Connection DAL
        services.AddScoped<IConnectionDal, ConnectionDal>();
        
        // Register ExecutionState DAL
        services.AddScoped<IExecutionStateDal, ExecutionStateDal>();
        
        // Register NodeType DAL
        services.AddScoped<INodeTypeDal, NodeTypeDal>();
        
        // Register Webhook DAL
        services.AddScoped<IWebhookDal, WebhookDal>();
        
        // Register Execution History DAL
        services.AddScoped<IExecutionHistoryDal, ExecutionHistoryDal>();
        
        return services;
    }
}