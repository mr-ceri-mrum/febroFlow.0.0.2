using FebroFlow.Business.Services;
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
            
            // Register OpenAI integration services
            
            // Register Azure OpenAI integration services
            services.AddScoped<IAzureOpenAiService, AzureOpenAIService>();
            
            // Register Vector Database integration services
            
            // Register Webhook integration services
            
            
            // Register Caching integration services
            services.AddScoped<ICachingService, CachingService>();

            // Register Pinecone integration services
           
            
            return services;
        }
    }
}