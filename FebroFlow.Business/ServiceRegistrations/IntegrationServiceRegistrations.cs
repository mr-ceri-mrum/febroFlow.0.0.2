using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FebroFlow.Business.ServiceRegistrations;

public static class IntegrationServiceRegistrations
{
    public static IServiceCollection AddIntegrationServices
        (this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Add HTTP client services
        services.AddHttpClient();
        
        // Telegram client setup
        services.Configure<TelegramOptions>(configuration.GetSection("Telegram"));
        
        // OpenAI client setup
        services.Configure<OpenAIOptions>(configuration.GetSection("OpenAI"));
        
        // Pinecone setup
        services.Configure<PineconeOptions>(configuration.GetSection("Pinecone"));
        
        return services;
    }
    
    public class TelegramOptions
    {
        public string BotToken { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
    }
    
    public class OpenAIOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
    }
    
    public class PineconeOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
    }
}