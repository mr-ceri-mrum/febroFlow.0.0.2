using FebroFlow.Business.Services.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FebroFlow.Business.ServiceRegistrations
{
    public static class ActionServiceRegistrations
    {
        public static IServiceCollection AddActionServices(
            this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            // Register AI Logic Actions
            services.AddScoped<IAIProcessingService, AIProcessingService>();
            services.AddScoped<ITextAnalysisService, TextAnalysisService>();
            services.AddScoped<IImageAnalysisService, ImageAnalysisService>();
            
            // Register Data Storage Actions
            services.AddScoped<IDataStorageService, DataStorageService>();
            services.AddScoped<IVectorStorageService, VectorStorageService>();
            
            // Register External API Actions
            services.AddScoped<IAPICallService, APICallService>();
            services.AddScoped<IWebhookCallService, WebhookCallService>();
            
            // Register Decision Actions
            services.AddScoped<IDecisionEngineService, DecisionEngineService>();
            services.AddScoped<IConditionEvaluatorService, ConditionEvaluatorService>();
            
            return services;
        }
    }
}