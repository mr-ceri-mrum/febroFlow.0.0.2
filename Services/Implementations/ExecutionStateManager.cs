using System.Text.Json;
using febroFlow.Core.Exceptions;
using febroFlow.DataAccess.DataAccess;
using febroFlow.DataAccess.DbModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace febroFlow.Business.Services.Implementations;

/// <summary>
/// Implementation of IExecutionStateManager for managing flow executions
/// </summary>
public class ExecutionStateManager : IExecutionStateManager
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExecutionStateManager> _logger;
    private readonly IExecutionHistoryDal _executionHistoryDal;
    private readonly TimeSpan _cacheExpirationTime = TimeSpan.FromHours(1);

    public ExecutionStateManager(
        IMemoryCache cache,
        ILogger<ExecutionStateManager> logger,
        IExecutionHistoryDal executionHistoryDal)
    {
        _cache = cache;
        _logger = logger;
        _executionHistoryDal = executionHistoryDal;
    }

    /// <inheritdoc />
    public async Task<string> InitializeExecution(Flow flow, Dictionary<string, object>? initialData = null)
    {
        // Create a unique execution ID
        string executionId = Guid.NewGuid().ToString();
        
        // Initialize the execution state
        var executionState = new Dictionary<string, object>
        {
            ["flowId"] = flow.Id,
            ["flowName"] = flow.Name,
            ["startTime"] = DateTime.UtcNow,
            ["isCompleted"] = false,
            ["currentStep"] = 0
        };
        
        // Add initial data if provided
        if (initialData != null)
        {
            foreach (var item in initialData)
            {
                executionState[item.Key] = item.Value;
            }
        }
        
        // Store the execution state in cache
        _cache.Set(GetCacheKey(executionId), executionState, _cacheExpirationTime);
        
        _logger.LogInformation("Initialized execution {ExecutionId} for flow {FlowName} ({FlowId})", 
            executionId, flow.Name, flow.Id);
        
        return executionId;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, object>> GetExecutionState(string executionId)
    {
        if (_cache.TryGetValue(GetCacheKey(executionId), out Dictionary<string, object> state))
        {
            return state;
        }
        
        _logger.LogWarning("Execution state not found for ID {ExecutionId}", executionId);
        throw new ExecutionNotFoundException($"Execution state not found for ID {executionId}");
    }

    /// <inheritdoc />
    public async Task UpdateExecutionState(string executionId, Dictionary<string, object> state)
    {
        // Update the timestamp
        state["lastUpdated"] = DateTime.UtcNow;
        
        // Store the updated state
        _cache.Set(GetCacheKey(executionId), state, _cacheExpirationTime);
        
        _logger.LogDebug("Updated execution state for {ExecutionId}", executionId);
    }

    /// <inheritdoc />
    public async Task CompleteExecution(string executionId, bool success, object? result = null)
    {
        try
        {
            // Get the current state
            var state = await GetExecutionState(executionId);
            
            // Mark as completed
            state["isCompleted"] = true;
            state["success"] = success;
            state["endTime"] = DateTime.UtcNow;
            
            if (result != null)
            {
                state["result"] = result;
            }
            
            // Save to the cache one last time
            _cache.Set(GetCacheKey(executionId), state, _cacheExpirationTime);
            
            // Create execution history record
            var history = new ExecutionHistory
            {
                Id = Guid.NewGuid(),
                FlowId = (Guid)state["flowId"],
                StartTime = (DateTime)state["startTime"],
                EndTime = (DateTime)state["endTime"],
                Success = success,
                ExecutionData = JsonSerializer.Serialize(state)
            };
            
            // Save to database
            await _executionHistoryDal.AddAsync(history);
            
            _logger.LogInformation("Completed execution {ExecutionId} with success={Success}", 
                executionId, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing execution {ExecutionId}", executionId);
            throw;
        }
    }
    
    /// <summary>
    /// Gets the cache key for the execution ID
    /// </summary>
    private string GetCacheKey(string executionId) => $"execution:{executionId}";
}