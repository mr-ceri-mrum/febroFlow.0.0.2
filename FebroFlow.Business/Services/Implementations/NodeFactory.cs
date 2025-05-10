using AutoMapper;
using FebroFlow.Data.Dtos.Node;
using FebroFlow.Data.Entities;
using FebroFlow.Data.Enums;
using FebroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DbModels;
using febroFlow.DataAccess.DataAccess;
using Microsoft.Extensions.Logging;

namespace FebroFlow.Business.Services.Implementations;

/// <summary>
/// Реализация фабрики для создания и управления узлами
/// </summary>
public class NodeFactory : INodeFactory
{
    private readonly INodeDal _nodeDal;
    private readonly IMapper _mapper;
    private readonly ILogger<NodeFactory> _logger;
    
    public NodeFactory(
        INodeDal nodeDal,
        IMapper mapper,
        ILogger<NodeFactory> logger)
    {
        _nodeDal = nodeDal;
        _mapper = mapper;
        _logger = logger;
    }
    
    /// <summary>
    /// Создание узла
    /// </summary>
    public async Task<Node> CreateNodeAsync(NodeCreateDto nodeDto)
    {
        _logger.LogInformation("Creating node with name {NodeName}", nodeDto.Name);
        
        // Создаем новый узел
        var node = new Node
        {
            Id = Guid.NewGuid(),
            Name = nodeDto.Name,
            Type = nodeDto.Type,
            Parameters = nodeDto.Parameters,
            PositionX = nodeDto.PositionX,
            PositionY = nodeDto.PositionY,
            FlowId = nodeDto.FlowId,
            CredentialId = nodeDto.CredentialId,
            DisableRetryOnFail = nodeDto.DisableRetryOnFail,
            AlwaysOutputData = nodeDto.AlwaysOutputData,
            DataCreate = DateTime.UtcNow
        };
        
        // Сохраняем в базу данных
        await _nodeDal.AddAsync(node);
        
        return node;
    }
    
    /// <summary>
    /// Обновление узла
    /// </summary>
    public async Task<Node> UpdateNodeAsync(NodeUpdateDto nodeDto)
    {
        _logger.LogInformation("Updating node {NodeId}", nodeDto.Id);
        
        // Получаем существующий узел
        var node = await _nodeDal.GetAsync(n => n.Id == nodeDto.Id);
        if (node == null)
        {
            throw new ArgumentException($"Node with ID {nodeDto.Id} not found");
        }
        
        // Обновляем его свойства
        node.Name = nodeDto.Name;
        node.Parameters = nodeDto.Parameters;
        node.PositionX = nodeDto.PositionX;
        node.PositionY = nodeDto.PositionY;
        node.CredentialId = nodeDto.CredentialId;
        node.DisableRetryOnFail = nodeDto.DisableRetryOnFail;
        node.AlwaysOutputData = nodeDto.AlwaysOutputData;
        node.ModifiedDate = DateTime.UtcNow;
        
        // Сохраняем изменения
        await _nodeDal.UpdateAsync(node);
        
        return node;
    }
    
    /// <summary>
    /// Удаление узла
    /// </summary>
    public async Task<bool> DeleteNodeAsync(Guid nodeId)
    {
        _logger.LogInformation("Deleting node {NodeId}", nodeId);
        
        // Получаем узел
        var node = await _nodeDal.GetAsync(n => n.Id == nodeId);
        if (node == null)
        {
            return false;
        }
        
        // Удаляем его
        await _nodeDal.DeleteAsync(node);
        
        return true;
    }
    
    /// <summary>
    /// Получение узла по ID
    /// </summary>
    public async Task<Node> GetNodeByIdAsync(Guid nodeId)
    {
        return await _nodeDal.GetAsync(n => n.Id == nodeId);
    }
    
    /// <summary>
    /// Получение всех узлов для потока
    /// </summary>
    public async Task<List<Node>> GetNodesByFlowIdAsync(Guid flowId)
    {
        return await _nodeDal.GetAllAsync(n => n.FlowId == flowId);
    }
    
    /// <summary>
    /// Получение доступных типов узлов
    /// </summary>
    public async Task<List<NodeTypeInfo>> GetNodeTypesAsync()
    {
        // Здесь можно было бы загружать типы узлов из базы данных или конфигурации
        // Для примера просто создадим список из перечисления
        
        var nodeTypes = Enum.GetValues<NodeType>()
            .Select(t => new NodeTypeInfo
            {
                Id = (int)t,
                Name = t.ToString(),
                Category = GetCategoryForNodeType(t),
                Description = GetDescriptionForNodeType(t),
                InputPorts = GetInputPortsForNodeType(t),
                OutputPorts = GetOutputPortsForNodeType(t)
            })
            .ToList();
            
        return nodeTypes;
    }
    
    /// <summary>
    /// Получение категории для типа узла
    /// </summary>
    private string GetCategoryForNodeType(NodeType type)
    {
        return type switch
        {
            NodeType.TelegramTrigger or NodeType.WebhookTrigger or NodeType.ScheduleTrigger => "Triggers",
            NodeType.OpenAI or NodeType.VectorStoreRetriever or NodeType.ChainRetrievalQA or NodeType.MemoryManager => "AI",
            NodeType.Switch or NodeType.Code or NodeType.SequentialThinking or NodeType.IfCondition => "Logic",
            NodeType.TelegramMessage or NodeType.HttpRequest or NodeType.DatabaseWrite or NodeType.FileOperation => "Actions",
            NodeType.EmbeddingsOpenAI or NodeType.PineconeVectorStore or NodeType.OpenAIChatModel => "AI Models",
            NodeType.ImageAnalysis or NodeType.AudioTranscription or NodeType.TextToSpeech => "Media Processing",
            _ => "Other"
        };
    }
    
    /// <summary>
    /// Получение описания для типа узла
    /// </summary>
    private string GetDescriptionForNodeType(NodeType type)
    {
        return type switch
        {
            NodeType.TelegramTrigger => "Triggers a flow when a Telegram message is received",
            NodeType.WebhookTrigger => "Triggers a flow when a webhook is called",
            NodeType.ScheduleTrigger => "Triggers a flow on a schedule",
            NodeType.OpenAI => "Processes text with OpenAI models",
            NodeType.VectorStoreRetriever => "Retrieves information from a vector database",
            NodeType.ChainRetrievalQA => "Performs question answering with document retrieval",
            NodeType.MemoryManager => "Manages conversation memory",
            NodeType.Switch => "Routes execution based on a condition",
            NodeType.Code => "Executes custom code",
            NodeType.SequentialThinking => "Breaks down complex problems into steps",
            NodeType.IfCondition => "Executes different paths based on a condition",
            NodeType.Translator => "Translates text between languages",
            NodeType.TelegramMessage => "Sends a message to Telegram",
            NodeType.HttpRequest => "Makes an HTTP request",
            NodeType.DatabaseWrite => "Writes data to a database",
            NodeType.FileOperation => "Performs file operations",
            NodeType.EmbeddingsOpenAI => "Generates embeddings using OpenAI",
            NodeType.PineconeVectorStore => "Stores vectors in Pinecone",
            NodeType.OpenAIChatModel => "Generates responses using OpenAI Chat models",
            NodeType.ImageAnalysis => "Analyzes images",
            NodeType.AudioTranscription => "Transcribes audio to text",
            NodeType.TextToSpeech => "Converts text to speech",
            _ => "Unknown node type"
        };
    }
    
    /// <summary>
    /// Получение входных портов для типа узла
    /// </summary>
    private List<PortInfo> GetInputPortsForNodeType(NodeType type)
    {
        // Упрощенная реализация
        return type switch
        {
            NodeType.TelegramTrigger or NodeType.WebhookTrigger or NodeType.ScheduleTrigger => new List<PortInfo>(),
            _ => new List<PortInfo> { new PortInfo { Name = "input", Label = "Input", Type = "any" } }
        };
    }
    
    /// <summary>
    /// Получение выходных портов для типа узла
    /// </summary>
    private List<PortInfo> GetOutputPortsForNodeType(NodeType type)
    {
        // Упрощенная реализация
        return type switch
        {
            NodeType.Switch => new List<PortInfo> 
            { 
                new PortInfo { Name = "true", Label = "True", Type = "any" },
                new PortInfo { Name = "false", Label = "False", Type = "any" }
            },
            NodeType.IfCondition => new List<PortInfo> 
            { 
                new PortInfo { Name = "then", Label = "Then", Type = "any" },
                new PortInfo { Name = "else", Label = "Else", Type = "any" }
            },
            _ => new List<PortInfo> { new PortInfo { Name = "output", Label = "Output", Type = "any" } }
        };
    }
}