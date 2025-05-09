using febroFlow.Business.Services;
using febroFlow.Business.Services.Implementations;
using febroFlow.Core.Responses;
using febroFlow.Core.Settings;
using febroFlow.DataAccess.DbContexts;
using febroFlow.Services;
using febroFlow.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace febroFlow.Business.ServiceRegistrations;

public static class CoreServiceRegistrations
{
    public static IServiceCollection AddCoreServices
        (this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        #region DbContexts
        var conn = "DefaultConnection";
        services.AddDbContext<FlowDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString(conn),
                b => b.MigrationsAssembly("febroFlow.API")
            );
        });
        #endregion

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddAuthorization();

        // Register message repository
        services.AddTransient<IMessagesRepository, MessagesRepository>();

        // Register OpenAI service
        services.Configure<OpenAISettings>(configuration.GetSection("OpenAI"));
        services.AddScoped<IOpenAIService, OpenAIService>();

        // Register Pinecone service
        services.AddScoped<IPineconeService, PineconeService>();

        // Register Telegram service
        services.Configure<TelegramSettings>(configuration.GetSection("Telegram"));
        services.AddScoped<ITelegramService, TelegramService>();

        // Register Flow services
        services.AddScoped<INodeFactory, NodeFactory>();
        services.AddScoped<IExecutionStateManager, ExecutionStateManager>();

        #region Cors
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
        #endregion

        return services;
    }
}