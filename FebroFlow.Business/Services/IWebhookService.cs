using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using FebroFlow.Core.ResultResponses;
using FebroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DbModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FebroFlow.Business.Services;

/// <summary>
/// Interface for webhook management and execution
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Register a new webhook for a flow
    /// </summary>
    /// <param name="flowId">ID of the flow to trigger</param>
    /// <param name="name">Name of the webhook</param>
    /// <param name="description">Description of the webhook</param>
    /// <param name="isActive">Whether the webhook is active</param>
    /// <returns>Generated webhook ID and secret</returns>
    Task<IDataResult<WebhookRegistrationResult>> RegisterWebhookAsync(Guid flowId, string name, string description, bool isActive = true);
    
    /// <summary>
    /// Update an existing webhook
    /// </summary>
    /// <param name="webhookId">ID of the webhook to update</param>
    /// <param name="name">New name for the webhook</param>
    /// <param name="description">New description for the webhook</param>
    /// <param name="isActive">New active status</param>
    /// <returns>Result of the operation</returns>
    Task<IResult> UpdateWebhookAsync(Guid webhookId, string name, string description, bool isActive);
    
    /// <summary>
    /// Delete a webhook
    /// </summary>
    /// <param name="webhookId">ID of the webhook to delete</param>
    /// <returns>Result of the operation</returns>
    Task<IResult> DeleteWebhookAsync(Guid webhookId);
    
    /// <summary>
    /// List all webhooks for a flow
    /// </summary>
    /// <param name="flowId">ID of the flow</param>
    /// <returns>List of webhooks for the flow</returns>
    Task<IDataResult<List<WebhookInfo>>> ListWebhooksForFlowAsync(Guid flowId);
    
    /// <summary>
    /// Verify a webhook request
    /// </summary>
    /// <param name="webhookId">ID of the webhook</param>
    /// <param name="signature">Signature from the request</param>
    /// <param name="payload">Payload of the request</param>
    /// <returns>True if the webhook is valid</returns>
    Task<IDataResult<bool>> VerifyWebhookAsync(Guid webhookId, string signature, string payload);
    
    /// <summary>
    /// Process an incoming webhook request and trigger the associated flow
    /// </summary>
    /// <param name="webhookId">ID of the webhook</param>
    /// <param name="payload">Payload from the request</param>
    /// <param name="headers">Headers from the request</param>
    /// <returns>Result of processing the webhook</returns>
    Task<IDataResult<Guid>> ProcessWebhookAsync(Guid webhookId, string payload, Dictionary<string, string> headers);
    
    /// <summary>
    /// Send a webhook call to an external URL
    /// </summary>
    /// <param name="url">URL to send the webhook to</param>
    /// <param name="payload">Payload to send</param>
    /// <param name="headers">Headers to include</param>
    /// <param name="method">HTTP method to use</param>
    /// <returns>Response from the webhook call</returns>
    Task<IDataResult<WebhookResponse>> SendWebhookAsync(
        string url, 
        object payload, 
        Dictionary<string, string>? headers = null, 
        HttpMethod? method = null);
}

/// <summary>
/// Represents a webhook configuration
/// </summary>
public class WebhookInfo
{
    public Guid Id { get; set; }
    public Guid FlowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public int TriggerCount { get; set; }
}

/// <summary>
/// Result of webhook registration
/// </summary>
public class WebhookRegistrationResult
{
    public Guid WebhookId { get; set; }
    public string WebhookUrl { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}

/// <summary>
/// Response from a webhook call
/// </summary>
public class WebhookResponse
{
    public int StatusCode { get; set; }
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// Implementation of Webhook service
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookService> _logger;
    private readonly IExecutionStateManager _executionStateManager;
    private readonly IConfiguration _configuration;
    private readonly IWebhookDal _webhookDal;
    
    public WebhookService(
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookService> logger,
        IExecutionStateManager executionStateManager,
        IConfiguration configuration,
        IWebhookDal webhookDal)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _executionStateManager = executionStateManager;
        _configuration = configuration;
        _webhookDal = webhookDal;
    }
    
    public async Task<IDataResult<WebhookRegistrationResult>> RegisterWebhookAsync(Guid flowId, string name, string description, bool isActive = true)
    {
        try
        {
            // Generate a secure random secret for webhook validation
            var secret = GenerateWebhookSecret();
            
            var webhook = new Webhook
            {
                Id = Guid.NewGuid(),
                FlowId = flowId,
                Name = name,
                Description = description,
                IsActive = isActive,
                Secret = secret,
                CreatedAt = DateTime.UtcNow,
                TriggerCount = 0
            };
            
            await _webhookDal.AddAsync(webhook);
            
            string baseUrl = _configuration["FebroFlow:WebhookBaseUrl"] ?? "https://api.febroflow.com/webhooks";
            var webhookUrl = $"{baseUrl.TrimEnd('/')}/{webhook.Id}";
            
            var result = new WebhookRegistrationResult
            {
                WebhookId = webhook.Id,
                WebhookUrl = webhookUrl,
                Secret = secret
            };
            
            return new SuccessDataResult<WebhookRegistrationResult>(result, "Webhook registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering webhook");
            return new ErrorDataResult<WebhookRegistrationResult>($"Error registering webhook: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<IResult> UpdateWebhookAsync(Guid webhookId, string name, string description, bool isActive)
    {
        try
        {
            var webhook = await _webhookDal.GetAsync(w => w.Id == webhookId);
            if (webhook == null)
            {
                return new ErrorResult("Webhook not found", System.Net.HttpStatusCode.NotFound);
            }
            
            webhook.Name = name;
            webhook.Description = description;
            webhook.IsActive = isActive;
            
            await _webhookDal.UpdateAsync(webhook);
            
            return new SuccessResult("Webhook updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating webhook");
            return new ErrorResult($"Error updating webhook: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<IResult> DeleteWebhookAsync(Guid webhookId)
    {
        try
        {
            var webhook = await _webhookDal.GetAsync(w => w.Id == webhookId);
            if (webhook == null)
            {
                return new ErrorResult("Webhook not found", System.Net.HttpStatusCode.NotFound);
            }
            
            await _webhookDal.DeleteAsync(webhook);
            
            return new SuccessResult("Webhook deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook");
            return new ErrorResult($"Error deleting webhook: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<IDataResult<List<WebhookInfo>>> ListWebhooksForFlowAsync(Guid flowId)
    {
        try
        {
            var webhooks = await _webhookDal.GetAllAsync(w => w.FlowId == flowId);
            
            var webhookInfos = webhooks.Select(w => new WebhookInfo
            {
                Id = w.Id,
                FlowId = w.FlowId,
                Name = w.Name,
                Description = w.Description,
                IsActive = w.IsActive,
                CreatedAt = w.CreatedAt,
                LastTriggeredAt = w.LastTriggeredAt,
                TriggerCount = w.TriggerCount
            }).ToList();
            
            return new SuccessDataResult<List<WebhookInfo>>(webhookInfos, "Webhooks retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing webhooks for flow");
            return new ErrorDataResult<List<WebhookInfo>>($"Error listing webhooks: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<IDataResult<bool>> VerifyWebhookAsync(Guid webhookId, string signature, string payload)
    {
        try
        {
            var webhook = await _webhookDal.GetAsync(w => w.Id == webhookId);
            if (webhook == null)
            {
                return new ErrorDataResult<bool>(false, "Webhook not found", System.Net.HttpStatusCode.NotFound);
            }
            
            // Compute expected signature
            var expectedSignature = ComputeSignature(webhook.Secret, payload);
            
            // Compare signatures (constant-time comparison to prevent timing attacks)
            bool isValid = SecureCompare(expectedSignature, signature);
            
            return new SuccessDataResult<bool>(isValid, isValid ? "Webhook signature is valid" : "Webhook signature is invalid");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook");
            return new ErrorDataResult<bool>(false, $"Error verifying webhook: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<IDataResult<Guid>> ProcessWebhookAsync(Guid webhookId, string payload, Dictionary<string, string> headers)
    {
        try
        {
            var webhook = await _webhookDal.GetAsync(w => w.Id == webhookId);
            if (webhook == null)
            {
                return new ErrorDataResult<Guid>(Guid.Empty, "Webhook not found", System.Net.HttpStatusCode.NotFound);
            }
            
            if (!webhook.IsActive)
            {
                return new ErrorDataResult<Guid>(Guid.Empty, "Webhook is not active", System.Net.HttpStatusCode.BadRequest);
            }
            
            // Update webhook statistics
            webhook.LastTriggeredAt = DateTime.UtcNow;
            webhook.TriggerCount++;
            await _webhookDal.UpdateAsync(webhook);
            
            // Parse the payload
            var payloadObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(payload);
            
            // Create initial state with webhook data
            var initialState = new Dictionary<string, object>
            {
                { "webhook", new Dictionary<string, object>
                    {
                        { "id", webhook.Id.ToString() },
                        { "payload", payloadObj ?? new Dictionary<string, object>() },
                        { "headers", headers }
                    }
                }
            };
            
            // Start flow execution
            var executionId = await _executionStateManager.StartFlowExecutionAsync(webhook.FlowId, initialState);
            
            return new SuccessDataResult<Guid>(executionId, "Webhook processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return new ErrorDataResult<Guid>(Guid.Empty, $"Error processing webhook: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }
    
    public async Task<IDataResult<WebhookResponse>> SendWebhookAsync(
        string url, 
        object payload, 
        Dictionary<string, string>? headers = null, 
        HttpMethod? method = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            
            // Prepare request with payload
            var content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json");
            
            // Set custom headers if provided
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (header.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue(header.Value);
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
            }
            
            // Send request
            var httpMethod = method ?? HttpMethod.Post;
            HttpResponseMessage response;
            
            if (httpMethod == HttpMethod.Get)
            {
                // For GET requests, serialize payload to query parameters
                var queryString = string.Join("&", JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    JsonConvert.SerializeObject(payload))?.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}") ?? Array.Empty<string>());
                
                var requestUrl = url;
                if (!string.IsNullOrEmpty(queryString))
                {
                    requestUrl += (url.Contains("?") ? "&" : "?") + queryString;
                }
                
                response = await client.GetAsync(requestUrl);
            }
            else
            {
                var request = new HttpRequestMessage(httpMethod, url)
                {
                    Content = content
                };
                
                response = await client.SendAsync(request);
            }
            
            // Process response
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseHeaders = response.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value));
            
            var webhookResponse = new WebhookResponse
            {
                StatusCode = (int)response.StatusCode,
                Content = responseContent,
                Headers = responseHeaders
            };
            
            return new SuccessDataResult<WebhookResponse>(webhookResponse, "Webhook sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook");
            return new ErrorDataResult<WebhookResponse>($"Error sending webhook: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }
    
    #region Helper Methods
    
    private string GenerateWebhookSecret()
    {
        var randomBytes = new byte[32]; // 256 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }
    
    private string ComputeSignature(string secret, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
    
    private bool SecureCompare(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }
        
        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        
        return result == 0;
    }
    
    #endregion
}