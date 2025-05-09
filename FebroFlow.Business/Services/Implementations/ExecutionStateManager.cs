using FebroFlow.Data.Enums;
using FebroFlow.DataAccess.DataAccess;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using FebroFlow.DataAccess.DbModels;

namespace FebroFlow.Business.Services.Implementations;

/// <summary>
/// Реализация менеджера состояния выполнения потока
/// </summary>
public class ExecutionStateManager : IExecutionStateManager
{
    private readonly IExecutionStateDal _executionStateDal;
    private readonly IFlowDal _flowDal;
    private readonly ILogger<ExecutionStateManager> _logger;
    
    public ExecutionStateManager(
        IExecutionStateDal executionStateDal,
        IFlowDal flowDal,
        ILogger<ExecutionStateManager> logger)
    {
        _executionStateDal = executionStateDal;
        _flowDal = flowDal;
        _logger = logger;
    }

    /// <summary>
    /// Инициализация нового состояния выполнения для потока
    /// </summary>
    public async Task<Guid> InitializeExecutionStateAsync(Guid flowId, object inputData)
    {
        _logger.LogInformation("Инициализация состояния выполнения для потока {FlowId}", flowId);
        
        // Проверка существования потока
        var flow = await _flowDal.GetAsync(f => f.Id == flowId);
        if (flow == null)
        {
            _logger.LogError("Поток {FlowId} не найден", flowId);
            throw new ArgumentException($"Поток {flowId} не найден");
        }
        
        // Сериализация входных данных
        string inputDataJson = JsonSerializer.Serialize(inputData);
        
        // Создание нового состояния выполнения
        var executionState = new ExecutionState
        {
            Id = Guid.NewGuid(),
            FlowId = flowId,
            Status = ExecutionStatus.Initialized,
            StartedAt = DateTime.UtcNow,
            CurrentNodeId = null,
            InputData = inputDataJson,
            OutputData = null,
            CurrentData = inputDataJson,
            ExecutionPath = new List<ExecutionPathItem>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        // Сохранение состояния в базу данных
        await _executionStateDal.AddAsync(executionState);
        
        _logger.LogInformation("Состояние выполнения {ExecutionId} создано для потока {FlowId}", executionState.Id, flowId);
        
        return executionState.Id;
    }

    /// <summary>
    /// Получение текущего состояния выполнения
    /// </summary>
    public async Task<ExecutionState> GetExecutionStateAsync(Guid executionId)
    {
        _logger.LogInformation("Получение состояния выполнения {ExecutionId}", executionId);
        
        // Получение состояния из базы данных
        var executionState = await _executionStateDal.GetAsync(e => e.Id == executionId);
        
        if (executionState == null)
        {
            _logger.LogError("Состояние выполнения {ExecutionId} не найдено", executionId);
            throw new ArgumentException($"Состояние выполнения {executionId} не найдено");
        }
        
        return executionState;
    }

    /// <summary>
    /// Обновление состояния выполнения
    /// </summary>
    public async Task<ExecutionState> UpdateExecutionStateAsync(Guid executionId, Guid currentNodeId, ExecutionStatus status, object data)
    {
        _logger.LogInformation("Обновление состояния выполнения {ExecutionId}, текущий узел: {NodeId}, статус: {Status}", 
            executionId, currentNodeId, status);
        
        // Получение текущего состояния
        var executionState = await GetExecutionStateAsync(executionId);
        
        // Сериализация данных
        string dataJson = JsonSerializer.Serialize(data);
        
        // Если текущий узел изменился, добавляем его в путь выполнения
        if (executionState.CurrentNodeId != currentNodeId)
        {
            executionState.ExecutionPath.Add(new ExecutionPathItem
            {
                NodeId = currentNodeId,
                Timestamp = DateTime.UtcNow,
                Data = dataJson
            });
        }
        
        // Обновление состояния
        executionState.CurrentNodeId = currentNodeId;
        executionState.Status = status;
        executionState.CurrentData = dataJson;
        executionState.UpdatedAt = DateTime.UtcNow;
        
        // Если выполнение завершено или прервано, устанавливаем время завершения
        if (status == ExecutionStatus.Completed || status == ExecutionStatus.Failed || status == ExecutionStatus.Cancelled)
        {
            executionState.FinishedAt = DateTime.UtcNow;
        }
        
        // Сохранение изменений
        await _executionStateDal.UpdateAsync(executionState);
        
        _logger.LogInformation("Состояние выполнения {ExecutionId} успешно обновлено", executionId);
        
        return executionState;
    }

    /// <summary>
    /// Завершение выполнения
    /// </summary>
    public async Task<ExecutionState> CompleteExecutionAsync(Guid executionId, ExecutionStatus status, object result)
    {
        _logger.LogInformation("Завершение выполнения {ExecutionId} со статусом {Status}", executionId, status);
        
        // Проверка статуса
        if (status != ExecutionStatus.Completed && status != ExecutionStatus.Failed && status != ExecutionStatus.Cancelled)
        {
            _logger.LogWarning("Неверный статус {Status} для завершения выполнения", status);
            status = ExecutionStatus.Completed; // По умолчанию устанавливаем статус Completed
        }
        
        // Получение текущего состояния
        var executionState = await GetExecutionStateAsync(executionId);
        
        // Сериализация результата
        string resultJson = JsonSerializer.Serialize(result);
        
        // Обновление состояния
        executionState.Status = status;
        executionState.OutputData = resultJson;
        executionState.FinishedAt = DateTime.UtcNow;
        executionState.UpdatedAt = DateTime.UtcNow;
        
        // Сохранение изменений
        await _executionStateDal.UpdateAsync(executionState);
        
        _logger.LogInformation("Выполнение {ExecutionId} успешно завершено со статусом {Status}", executionId, status);
        
        return executionState;
    }

    /// <summary>
    /// Получение истории выполнения для потока
    /// </summary>
    public async Task<List<ExecutionState>> GetExecutionHistoryAsync(Guid flowId)
    {
        _logger.LogInformation("Получение истории выполнения для потока {FlowId}", flowId);
        
        // Получение всех состояний выполнения для указанного потока
        var executionStates = await _executionStateDal.GetAllAsync(
            e => e.FlowId == flowId,
            e => e.StartedAt,
            true);
        
        _logger.LogInformation("Найдено {Count} записей истории выполнения для потока {FlowId}", executionStates.Count, flowId);
        
        return executionStates;
    }
}