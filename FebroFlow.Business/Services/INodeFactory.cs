using FebroFlow.Data.Dtos.Node;
using FebroFlow.Data.Entities;
using FebroFlow.DataAccess.DbModels;

namespace FebroFlow.Business.Services;

/// <summary>
/// Интерфейс фабрики для создания и управления узлами
/// </summary>
public interface INodeFactory
{
    /// <summary>
    /// Создание узла
    /// </summary>
    /// <param name="nodeDto">DTO узла</param>
    /// <returns>Созданный узел</returns>
    Task<Node> CreateNodeAsync(NodeCreateDto nodeDto);
    
    /// <summary>
    /// Обновление узла
    /// </summary>
    /// <param name="nodeDto">DTO узла</param>
    /// <returns>Обновленный узел</returns>
    Task<Node> UpdateNodeAsync(NodeUpdateDto nodeDto);
    
    /// <summary>
    /// Удаление узла
    /// </summary>
    /// <param name="nodeId">ID узла</param>
    /// <returns>Результат удаления</returns>
    Task<bool> DeleteNodeAsync(Guid nodeId);
    
    /// <summary>
    /// Получение узла по ID
    /// </summary>
    /// <param name="nodeId">ID узла</param>
    /// <returns>Узел</returns>
    Task<Node> GetNodeByIdAsync(Guid nodeId);
    
    /// <summary>
    /// Получение всех узлов для потока
    /// </summary>
    /// <param name="flowId">ID потока</param>
    /// <returns>Список узлов</returns>
    Task<List<Node>> GetNodesByFlowIdAsync(Guid flowId);
    
    /// <summary>
    /// Получение доступных типов узлов
    /// </summary>
    /// <returns>Список типов узлов</returns>
    Task<List<NodeTypeInfo>> GetNodeTypesAsync();
}