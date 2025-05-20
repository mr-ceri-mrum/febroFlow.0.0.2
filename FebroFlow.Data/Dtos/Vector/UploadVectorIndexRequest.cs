using Microsoft.AspNetCore.Http;

namespace FebroFlow.Data.Dtos.Vector;

public class UploadVectorCollectionRequest
{
    public string id { get; set; }
    public string CollectionName {get;set;} 
    public IFormFile File { get; set; }
}