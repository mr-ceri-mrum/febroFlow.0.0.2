using FebroFlow.Business.UseCase.Flow;
using FebroFlow.Business.UseCase.Node;
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
            #region Flow

            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(FlowCreateCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(FlowGetCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(FlowUpdateCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(FlowDeleteCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(FlowExecuteCommand).Assembly));

            #endregion

            #region Node

            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(NodeCreateCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(NodeGetCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(NodeUpdateCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(NodeDeleteCommand).Assembly));

            #endregion
            
            return services;
        }
    }
}