using System.Net;
using febroFlow.Core.Dtos;
using febroFlow.Core.Dtos.Flow;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace febroFlow.Business.UseCase.Flow;

/// <summary>
/// Command to get all flows with pagination
/// </summary>
public class FlowGetAllCommand : IRequest<IDataResult<PagedResult<FlowDto>>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public bool IncludeInactive { get; }

    public FlowGetAllCommand(int pageNumber = 1, int pageSize = 10, bool includeInactive = false)
    {
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize > 0 ? pageSize : 10;
        IncludeInactive = includeInactive;
    }
}

/// <summary>
/// Handler for FlowGetAllCommand
/// </summary>
public class FlowGetAllCommandHandler : IRequestHandler<FlowGetAllCommand, IDataResult<PagedResult<FlowDto>>>
{
    private readonly IFlowDal _flowDal;
    private readonly IMessagesRepository _messagesRepository;

    public FlowGetAllCommandHandler(
        IFlowDal flowDal,
        IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to get all flows with pagination
    /// </summary>
    public async Task<IDataResult<PagedResult<FlowDto>>> Handle(FlowGetAllCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create the base query
            var query = _flowDal.GetAllAsQueryable();

            // Apply filtering
            if (!request.IncludeInactive)
            {
                query = query.Where(f => f.IsActive);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var flows = await query
                .OrderByDescending(f => f.ModifiedDate ?? f.DataCreate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(f => new FlowDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    IsActive = f.IsActive,
                    CreatedAt = f.DataCreate,
                    ModifiedAt = f.ModifiedDate
                })
                .ToListAsync(cancellationToken);

            // Create paged result
            var result = new PagedResult<FlowDto>
            {
                Items = flows,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return new SuccessDataResult<PagedResult<FlowDto>>(result, "Flows retrieved successfully");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<PagedResult<FlowDto>>(
                $"Error retrieving flows: {ex.Message}",
                HttpStatusCode.InternalServerError);
        }
    }
}