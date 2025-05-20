using Microsoft.AspNetCore.Http;

namespace FebroFlow.Data.Dtos.Vector;

public class CreateVectorIndexRequest
{
    public string IndexName { get; set; }
    public Guid UserId { get; set; }
    public IFormFile? File { get; set; } 
    public Guid OrganizationId { get; set; }
}