using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace FebroFlow.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FlowExecutionController : ControllerBase
{
    private readonly IFlowEngine _flowEngine;
    private readonly IExecutionStateManager _executionStateManager;
    private readonly ILogger<FlowExecutionController> _logger;
    
    public FlowExecutionController(
        IFlowEngine flowEngine,
        IExecutionStateManager executionStateManager,
        ILogger<FlowExecutionController> logger)
    {
        _flowEngine = flowEngine;
        _executionStateManager = executionStateManager;
        _logger = logger;
    }
    
    /// <summary>
    /// Execute a flow with the given input data
    /// </summary>
    [HttpPost("execute/{flowId}")]
    public async Task<IActionResult> ExecuteFlow(Guid flowId, [FromBody] JsonElement inputData)
    {
        try
        {
            // Convert JsonElement to Dictionary
            var inputDict = JsonSerializer.Deserialize<Dictionary<string, object>>(inputData.GetRawText()) ?? new Dictionary<string, object>();
            
            // Generate a context ID based on the request
            var contextId = $"request-{Guid.NewGuid()}";
            
            // Execute the flow
            var executionId = await _flowEngine.ExecuteFlowAsync(flowId, contextId, inputDict);
            
            return Ok(new SuccessDataResult<object>(
                new { executionId }, 
                "Flow execution started successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing flow {FlowId}", flowId);
            return StatusCode(500, new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError));
        }
    }
    
    /// <summary>
    /// Get the state of a flow execution
    /// </summary>
    [HttpGet("state/{executionId}")]
    public async Task<IActionResult> GetExecutionState(Guid executionId)
    {
        try
        {
            var executionState = await _flowEngine.GetFlowExecutionStateAsync(executionId);
            
            return Ok(new SuccessDataResult<object>(
                new
                {
                    executionId = executionState.Id,
                    flowId = executionState.FlowId,
                    status = executionState.Status.ToString(),
                    startedAt = executionState.StartedAt,
                    finishedAt = executionState.FinishedAt,
                    currentNodeId = executionState.CurrentNodeId,
                    // Parse the JSON stored in the execution state
                    output = JsonSerializer.Deserialize<object>(executionState.OutputData)
                }, 
                "Execution state retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution state {ExecutionId}", executionId);
            return StatusCode(500, new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError));
        }
    }
    
    /// <summary>
    /// Cancel a flow execution
    /// </summary>
    [HttpPost("cancel/{executionId}")]
    public async Task<IActionResult> CancelExecution(Guid executionId)
    {
        try
        {
            await _flowEngine.CancelFlowExecutionAsync(executionId);
            
            return Ok(new SuccessResult("Flow execution cancelled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling execution {ExecutionId}", executionId);
            return StatusCode(500, new ErrorResult(ex.Message, HttpStatusCode.InternalServerError));
        }
    }
    
    /// <summary>
    /// Get the execution history for a flow
    /// </summary>
    [HttpGet("history/{flowId}")]
    public async Task<IActionResult> GetExecutionHistory(Guid flowId)
    {
        try
        {
            var history = await _executionStateManager.GetExecutionHistoryAsync(flowId);
            
            var historyDtos = history.Select(h => new
            {
                executionId = h.Id,
                status = h.Status.ToString(),
                startedAt = h.StartedAt,
                finishedAt = h.FinishedAt,
                currentNodeId = h.CurrentNodeId
            }).ToList();
            
            return Ok(new SuccessDataResult<object>(
                historyDtos, 
                "Execution history retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution history for flow {FlowId}", flowId);
            return StatusCode(500, new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError));
        }
    }
}