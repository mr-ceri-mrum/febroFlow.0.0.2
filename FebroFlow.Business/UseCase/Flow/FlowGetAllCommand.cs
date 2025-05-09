using System.Net;
using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using FebroFlow.Data.Dtos.Paginate;
using FebroFlow.DataAccess.DataAccess;
using MediatR;

namespace FebroFlow.Business.UseCase.Flow;

/// <summary>
/// Команда для получения списка потоков
/// </summary>
public class FlowGetAllCommand : IRequest<IDataResult<object>>
{
    /// <summary>
    /// Параметры пагинации
    /// </summary>
    public PaginationDto Pagination { get; }
    
    /// <summary>
    /// Искать только публичные потоки
    /// </summary>
    public bool OnlyPublic { get; }

    public FlowGetAllCommand(PaginationDto pagination, bool onlyPublic = false)
    {
        Pagination = pagination;
        OnlyPublic = onlyPublic;
    }
}

/// <summary>
/// Обработчик команды получения списка потоков
/// </summary>
public class FlowGetAllCommandHandler : IRequestHandler<FlowGetAllCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly IAuthInformationRepository _authInformationRepository;
    private readonly IMapper _mapper;

    public FlowGetAllCommandHandler(
        IFlowDal flowDal,
        IAuthInformationRepository authInformationRepository,
        IMapper mapper)
    {
        _flowDal = flowDal;
        _authInformationRepository = authInformationRepository;
        _mapper = mapper;
    }

    public async Task<IDataResult<object>> Handle(FlowGetAllCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _authInformationRepository.GetUserId();
            
            // Проверка аутентификации только если запрашиваются личные потоки
            if (userId == Guid.Empty && !request.OnlyPublic)
            {
                return new ErrorDataResult<object>("Необходима аутентификация", HttpStatusCode.Unauthorized);
            }
            
            // Получаем потоки в зависимости от параметров
            var flows = request.OnlyPublic
                ? await _flowDal.GetAllAsync(f => f.IsPublic) // Только публичные
                : await _flowDal.GetAllAsync(f => f.UserId == userId || f.IsPublic); // Свои и публичные
            
            // Применяем пагинацию
            var pagedFlows = flows
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .ToList();
            
            // Маппим на DTO
            var flowDtos = _mapper.Map<List<FlowDto>>(pagedFlows);
            
            // Создаем результат с пагинацией
            var result = new PaginatedResultDto<FlowDto>
            {
                Items = flowDtos,
                TotalCount = flows.Count,
                PageNumber = request.Pagination.PageNumber,
                PageSize = request.Pagination.PageSize,
                TotalPages = (int)Math.Ceiling(flows.Count / (double)request.Pagination.PageSize)
            };
            
            return new SuccessDataResult<object>(result, "");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}