using System.Net;
using System.Text;
using System.Text.Json;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Vector;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FebroFlow.Business.UseCase.Vector;

public class VectorIndexCreateCommand(CreateVectorIndexRequest form) : IRequest<IDataResult<object>>
{
    public CreateVectorIndexRequest Form { get; set; } = form;
}

public class VectorIndexCreateCommandHandler(
    IConfiguration configuration, IQdrantService qdrantService, IVectorDal vectorDal) 
    
    : IRequestHandler<VectorIndexCreateCommand, IDataResult<object>>
{
        public async Task<IDataResult<object>> Handle(VectorIndexCreateCommand request, CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient();
            string collectionName = request.Form.IndexName;
            string url = $"http://localhost:6333/collections/{collectionName}";
            
            var requestBody = new
            {
                vectors = new
                {
                    size = 1536,
                    distance = "Cosine"
                }
            };
            
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
                
            try
            {
                var response = await httpClient.PutAsync(url, content, cancellationToken); // Qdrant expects PUT for create

                if (response.IsSuccessStatusCode)
                {
                    var vector = new DataAccess.DbModels.Vector
                    {
                        CreatorId = Guid.NewGuid(),
                        IndexName = request.Form.IndexName,
                        OrganizationId = request.Form.OrganizationId,
                    };
                    
                    await vectorDal.AddAsync(vector);
                    return new SuccessDataResult<object>("Collection created successfully.");
                } 
                
                var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                return new ErrorDataResult<object>($"Failed to create collection. Status: {response.StatusCode}. Error: {errorMessage}", HttpStatusCode.ServiceUnavailable);
                
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<object>($"Exception occurred: {ex.Message}", HttpStatusCode.ServiceUnavailable);
            }
        }
}