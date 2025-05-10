using FebroFlow.Business.Services;
using FebroFlow.Business.Services.Implementations;
using FebroFlow.Core.Responses;
using FebroFlow.DataAccess.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FebroFlow.Business.ServiceRegistrations;

public static class CoreServiceRegistrations
{
    public static IServiceCollection AddCoreServices
        (this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // DbContext
        var connectionName = "DefaultConnection";
        services.AddDbContext<DataContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString(connectionName),
                b => b.MigrationsAssembly("FebroFlow.API")
            );
        });
        
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        
        // Register core services
        services.AddScoped<IMessagesRepository, MessagesRepository>();
        services.AddScoped<IFlowEngine, FlowEngine>();
        services.AddScoped<INodeFactory, NodeFactory>();
        services.AddScoped<IConnectionManager, ConnectionManager>();
        services.AddScoped<IExecutionStateManager, ExecutionStateManager>();
        services.AddScoped<ITelegramService, TelegramService>();
        services.AddScoped<IOpenAIService, OpenAIService>();
        services.AddScoped<IPineconeService, PineconeService>();
        
        // CORS setup
        services.AddCors(options =>
        {
            options.AddPolicy(name: configuration.GetSection("CorsLabel").Value!,
                builder =>
                {
                    builder.WithMethods(
                        configuration.GetSection("Methods").GetChildren().Select(x => x.Value).ToArray()!);
                    builder.AllowAnyHeader();
                    builder.AllowCredentials();
                    builder.WithOrigins(
                        configuration.GetSection("Origins").GetChildren().Select(x => x.Value).ToArray()!);
                    builder.Build();
                });
        });
        
        return services;
    }
}