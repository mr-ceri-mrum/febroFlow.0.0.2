using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Node;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using System.Net;
using FebroFlow.Core.Responses;
using febroFlow.DataAccess.DataAccess;

namespace FebroFlow.Business.UseCase.Node;

public class NodeGetCommand : IRequest<IDataResult<object>>
{
    public Guid Id { get; }

    public NodeGetCommand(Guid id)
    {
        Id = id;
    }
}

public class NodeGetCommandHandler : IRequestHandler<NodeGetCommand, IDataResult<object>>
{
    private readonly INodeDal _nodeDal;
    private readonly IMapper _mapper;
    private readonly IMessagesRepository _messagesRepository;
    
    public NodeGetCommandHandler(
        INodeDal nodeDal,
        IMapper mapper,
        IMessagesRepository messagesRepository)
    {
        _nodeDal = nodeDal;
        _mapper = mapper;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(NodeGetCommand request, CancellationToken cancellationToken)
    {
        // Retrieve node
        var node = await _nodeDal.GetAsync(x => x.Id == request.Id);
        
        if (node == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Node"), HttpStatusCode.NotFound);
        }
        
        // Map to DTO
        var nodeDto = _mapper.Map<NodeDto>(node);
        
        return new SuccessDataResult<object>(nodeDto, "Node retrieved successfully");
    }
}