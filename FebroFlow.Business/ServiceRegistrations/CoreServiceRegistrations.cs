using FebroFlow.Business.Services;
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
        #region DbContexts
        var conn = "DefaultConnection";
        services.AddDbContext<DataContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString(conn),
                 b => b.MigrationsAssembly("FebroFlow.DataAccess"));
        });
        #endregion

       
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        // Register message repository
        services.AddTransient<IMessagesRepository, MessagesRepository>();

        // Register HTTP client factory
        services.AddHttpClient();
        
        // Register memory cache
        services.AddMemoryCache();

        // Register AuthInformationRepository
        services.AddScoped<IAuthInformationRepository, AuthInformationRepository>();

        // Register ConnectionManager

        // Register OpenAI service
       

        // Register Pinecone service
        services.AddScoped<IPineconeService, PineconeService>();
       
        
        // Register Telegram service
       

        // Register Flow services
       
        // Register VectorDatabaseService
       

        // Register WebhookService
       

        // Register ImageAnalysisService

        // Register AzureOpenAIService 
      

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

        services.AddScoped<IOpenAiService, OpenAiService>();
        return services;
    }
}