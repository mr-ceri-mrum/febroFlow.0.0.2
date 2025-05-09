using FebroFlow.Data.Enums;
using FebroFlow.DataAccess.DbModels;

namespace FebroFlow.Business.Services;

/// <summary>
/// Интерфейс для управления состоянием выполнения потока
/// </summary>
public interface IExecutionStateManager
{
    /// <summary>
    /// Инициализация нового состояния выполнения для потока
    /// </summary>
    /// <param name="flowId">ID потока</param>
    /// <param name="inputData">Входные данные</param>
    /// <returns>ID состояния выполнения</returns>
    Task<Guid> InitializeExecutionStateAsync(Guid flowId, object inputData);
    
    /// <summary>
    /// Получение текущего состояния выполнения
    /// </summary>
    /// <param name="executionId">ID выполнения</param>
    /// <returns>Состояние выполнения</returns>
    Task<ExecutionState> GetExecutionStateAsync(Guid executionId);
    
    /// <summary>
    /// Обновление состояния выполнения
    /// </summary>
    /// <param name="executionId">ID выполнения</param>
    /// <param name="currentNodeId">ID текущего узла</param>
    /// <param name="status">Статус выполнения</param>
    /// <param name="data">Данные выполнения</param>
    /// <returns>Обновленное состояние выполнения</returns>
    Task<ExecutionState> UpdateExecutionStateAsync(Guid executionId, Guid currentNodeId, ExecutionStatus status, object data);
    
    /// <summary>
    /// Завершение выполнения
    /// </summary>
    /// <param name="executionId">ID выполнения</param>
    /// <param name="status">Статус выполнения</param>
    /// <param name="result">Результат выполнения</param>
    /// <returns>Финальное состояние выполнения</returns>
    Task<ExecutionState> CompleteExecutionAsync(Guid executionId, ExecutionStatus status, object result);
    
    /// <summary>
    /// Получение истории выполнения для потока
    /// </summary>
    /// <param name="flowId">ID потока</param>
    /// <returns>Список состояний выполнения</returns>
    Task<List<ExecutionState>> GetExecutionHistoryAsync(Guid flowId);
}