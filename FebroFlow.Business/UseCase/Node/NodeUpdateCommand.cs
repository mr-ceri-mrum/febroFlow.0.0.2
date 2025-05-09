using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Node;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using System.Net;

namespace FebroFlow.Business.UseCase.Node;

public class NodeUpdateCommand : IRequest<IDataResult<object>>
{
    public NodeUpdateDto Form { get; }

    public NodeUpdateCommand(NodeUpdateDto form)
    {
        Form = form;
    }
}

public class NodeUpdateCommandHandler : IRequestHandler<NodeUpdateCommand, IDataResult<object>>
{
    private readonly INodeDal _nodeDal;
    private readonly IMapper _mapper;
    private readonly IMessagesRepository _messagesRepository;
    
    public NodeUpdateCommandHandler(
        INodeDal nodeDal,
        IMapper mapper,
        IMessagesRepository messagesRepository)
    {
        _nodeDal = nodeDal;
        _mapper = mapper;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(NodeUpdateCommand request, CancellationToken cancellationToken)
    {
        // Check if node exists
        var node = await _nodeDal.GetAsync(x => x.Id == request.Form.Id);
        
        if (node == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Node"), HttpStatusCode.NotFound);
        }
        
        // Update properties
        node.Name = request.Form.Name;
        node.Parameters = request.Form.Parameters;
        node.PositionX = request.Form.PositionX;
        node.PositionY = request.Form.PositionY;
        node.CredentialId = request.Form.CredentialId;
        node.DisableRetryOnFail = request.Form.DisableRetryOnFail;
        node.AlwaysOutputData = request.Form.AlwaysOutputData;
        node.ModifiedDate = DateTime.Now;
        
        // Save changes
        await _nodeDal.UpdateAsync(node);
        
        return new SuccessDataResult<object>(node, _messagesRepository.Edited("Node"));
    }
}