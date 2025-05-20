using AutoMapper;
using FebroFlow.Data.Dtos.Flow;
using FebroFlow.Data.Dtos.Node;
using FebroFlow.DataAccess.DbModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FebroFlow.Business.ServiceRegistrations;

public static class AutoMapServiceRegistrations
{
    public static IServiceCollection AddAutoMapServices
        (this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddSingleton(new MapperConfiguration(config =>
        {
            #region Flows
            config.CreateMap<FlowCreateDto, Flow>();
            config.CreateMap<FlowUpdateDto, Flow>();
            config.CreateMap<Flow, FlowDto>();
            config.CreateMap<CreateFlowRequest, Flow>();
            #endregion
            
            #region Nodes
            config.CreateMap<NodeCreateDto, Node>();
            config.CreateMap<NodeUpdateDto, Node>();
            config.CreateMap<Node, NodeDto>();
            #endregion
            
            #region Connections
            config.CreateMap<ConnectionCreateDto, Connection>();
            config.CreateMap<ConnectionUpdateDto, Connection>();
            config.CreateMap<Connection, ConnectionDto>();
            #endregion
        }).CreateMapper());

       
        return services;
    }
}