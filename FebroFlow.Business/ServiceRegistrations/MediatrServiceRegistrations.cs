using FebroFlow.Business.UseCase.Flow;
using FebroFlow.Business.UseCase.Vector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FebroFlow.Business.ServiceRegistrations
{
    public static class MediatrServiceRegistrations
    {
        public static IServiceCollection AddMediatrServices(
            this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            // Регистрируем MediatR только один раз
            services.AddMediatR(cnf => {
                cnf.RegisterServicesFromAssemblies(typeof(FlowCreateCommand).Assembly);
                cnf.RegisterServicesFromAssemblies(typeof(FlowTestCommand).Assembly);
                cnf.RegisterServicesFromAssemblies(typeof(VectorIndexCreateCommand).Assembly);
                cnf.RegisterServicesFromAssemblies(typeof(VectorUploadCollectionCreateCommand).Assembly);
                cnf.RegisterServicesFromAssemblies(typeof(VectorGetCollectionsCommand).Assembly);
                cnf.RegisterServicesFromAssemblies(typeof(VectorSearchByTextCommand).Assembly);
            });
            
            // Удаляем все повторные вызовы, которые вызывают ошибку
            
            return services;
        }
    }
}