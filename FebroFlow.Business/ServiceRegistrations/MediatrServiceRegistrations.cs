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
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(FlowUpdateCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(FlowDeleteCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(FlowExecuteCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(FlowTestCommand).Assembly));
            #endregion
        
            #region Node

            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(NodeCreateCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(NodeGetCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(NodeUpdateCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(NodeDeleteCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(NodeGetTypesCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(NodeGetByFlowCommand).Assembly));

            
            #endregion

            #region Con
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(ConnectionCreateCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(ConnectionDeleteCommand).Assembly));
            services.AddMediatR(cnf => cnf.RegisterServicesFromAssemblies(typeof(ConnectionUpdateCommand).Assembly));
    
            

            #endregion
            return services;
        }
    }
}