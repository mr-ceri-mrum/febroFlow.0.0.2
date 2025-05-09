using FebroFlow.DataAccess.DbModels;

namespace FebroFlow.Business.Services;

/// <summary>
/// Интерфейс для управления соединениями между узлами
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// Создание соединения между узлами
    /// </summary>
    /// <param name="sourceNodeId">ID исходного узла</param>
    /// <param name="targetNodeId">ID целевого узла</param>
    /// <param name="flowId">ID потока</param>
    /// <returns>Созданное соединение</returns>
    Task<Connection> CreateConnectionAsync(Guid sourceNodeId, Guid targetNodeId, Guid flowId);
    
    /// <summary>
    /// Обновление соединения
    /// </summary>
    /// <param name="connection">Обновленное соединение</param>
    /// <returns>Обновленное соединение</returns>
    Task<Connection> UpdateConnectionAsync(Connection connection);
    
    /// <summary>
    /// Удаление соединения
    /// </summary>
    /// <param name="connectionId">ID соединения</param>
    /// <returns>Результат удаления</returns>
    Task<bool> DeleteConnectionAsync(Guid connectionId);
    
    /// <summary>
    /// Получение всех соединений для потока
    /// </summary>
    /// <param name="flowId">ID потока</param>
    /// <returns>Список соединений</returns>
    Task<List<Connection>> GetConnectionsByFlowIdAsync(Guid flowId);
    
    /// <summary>
    /// Проверка валидности соединения
    /// </summary>
    /// <param name="sourceNodeId">ID исходного узла</param>
    /// <param name="targetNodeId">ID целевого узла</param>
    /// <returns>Результат проверки</returns>
    Task<bool> ValidateConnectionAsync(Guid sourceNodeId, Guid targetNodeId);
}