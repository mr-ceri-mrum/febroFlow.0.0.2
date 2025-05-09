using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FebroFlow.Business.UseCase.Flow;

public class FlowGetAllCommand : IRequest<IDataResult<object>>
{
    public int PageNumber { get; }
    public int PageSize { get; }

    public FlowGetAllCommand(int pageNumber = 1, int pageSize = 10)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}

public class FlowGetAllCommandHandler : IRequestHandler<FlowGetAllCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly IMapper _mapper;
    private readonly IMessagesRepository _messagesRepository;
    
    public FlowGetAllCommandHandler(IFlowDal flowDal, IMapper mapper, IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _mapper = mapper;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(FlowGetAllCommand request, CancellationToken cancellationToken)
    {
        // Retrieve paginated flows
        var flows = await _flowDal.GetAllAsQueryable(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);
        
        // Map to DTOs
        var flowDtos = _mapper.Map<List<FlowDto>>(flows);
        
        // Get total count for pagination info
        var totalCount = await _flowDal.CountAsync();
        
        var result = new
        {
            Items = flowDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
        
        return new SuccessDataResult<object>(result, "Flows retrieved successfully");
    }
}