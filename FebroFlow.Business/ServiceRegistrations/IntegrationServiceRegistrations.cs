using FebroFlow.Business.Services.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FebroFlow.Business.ServiceRegistrations
{
    public static class IntegrationServiceRegistrations
    {
        public static IServiceCollection AddIntegrationServices(
            this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            // Register Telegram integration services
            services.AddScoped<ITelegramService, TelegramService>();
            
            // Register OpenAI integration services
            services.AddScoped<IOpenAIService, OpenAIService>();
            
            // Register Azure OpenAI integration services
            services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();
            
            // Register Vector Database integration services
            services.AddScoped<IVectorDatabaseService, VectorDatabaseService>();
            
            // Register Webhook integration services
            services.AddScoped<IWebhookService, WebhookService>();
            
            // Register Caching integration services
            services.AddScoped<ICachingService, CachingService>();

            // Register Pinecone integration services
            services.AddScoped<IPineconeService, PineconeService>();
            
            return services;
        }
    }
}