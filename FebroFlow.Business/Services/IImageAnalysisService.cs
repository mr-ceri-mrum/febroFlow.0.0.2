using System.Text;
using FebroFlow.Core.ResultResponses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FebroFlow.Business.Services;

/// <summary>
/// Interface for Image Analysis Service
/// </summary>
public interface IImageAnalysisService
{
    /// <summary>
    /// Analyzes an image to extract objects, text, faces, and other visual elements
    /// </summary>
    /// <param name="imageData">The image data as a byte array</param>
    /// <param name="features">List of features to analyze</param>
    /// <returns>Analysis results containing detected elements</returns>
    Task<IDataResult<ImageAnalysisResult>> AnalyzeImageAsync(byte[] imageData, ImageAnalysisFeatures features);
    
    /// <summary>
    /// Extracts text (OCR) from an image
    /// </summary>
    /// <param name="imageData">The image data as a byte array</param>
    /// <returns>Extracted text from the image</returns>
    Task<IDataResult<string>> ExtractTextFromImageAsync(byte[] imageData);
    
    /// <summary>
    /// Generates a textual description of the image content
    /// </summary>
    /// <param name="imageData">The image data as a byte array</param>
    /// <returns>A textual description of the image</returns>
    Task<IDataResult<string>> GenerateImageDescriptionAsync(byte[] imageData);
    
    /// <summary>
    /// Detects objects in an image and their bounding boxes
    /// </summary>
    /// <param name="imageData">The image data as a byte array</param>
    /// <returns>List of detected objects with confidence scores and locations</returns>
    Task<IDataResult<List<DetectedObject>>> DetectObjectsAsync(byte[] imageData);
}

/// <summary>
/// Features to analyze in an image
/// </summary>
[Flags]
public enum ImageAnalysisFeatures
{
    None = 0,
    Objects = 1,
    Text = 2,
    Faces = 4,
    Labels = 8,
    Description = 16,
    All = Objects | Text | Faces | Labels | Description
}

/// <summary>
/// Result of image analysis
/// </summary>
public class ImageAnalysisResult
{
    public List<DetectedObject>? Objects { get; set; }
    public string? ExtractedText { get; set; }
    public List<DetectedFace>? Faces { get; set; }
    public List<ImageLabel>? Labels { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Represents an object detected in an image
/// </summary>
public class DetectedObject
{
    public string Name { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public BoundingBox Box { get; set; } = new();
}

/// <summary>
/// Represents a face detected in an image
/// </summary>
public class DetectedFace
{
    public BoundingBox Box { get; set; } = new();
    public Dictionary<string, float> Attributes { get; set; } = new();
}

/// <summary>
/// Represents a label (tag) associated with an image
/// </summary>
public class ImageLabel
{
    public string Name { get; set; } = string.Empty;
    public float Confidence { get; set; }
}

/// <summary>
/// Represents a bounding box in an image
/// </summary>
public class BoundingBox
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// Implementation of Image Analysis Service using Azure Computer Vision
/// </summary>
public class AzureImageAnalysisService : IImageAnalysisService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureImageAnalysisService> _logger;

    public AzureImageAnalysisService(IHttpClientFactory httpClientFactory, 
                                    IConfiguration configuration,
                                    ILogger<AzureImageAnalysisService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IDataResult<ImageAnalysisResult>> AnalyzeImageAsync(byte[] imageData, ImageAnalysisFeatures features)
    {
        try
        {
            string? apiKey = _configuration["AzureComputerVision:ApiKey"];
            string? endpoint = _configuration["AzureComputerVision:Endpoint"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
            {
                return new ErrorDataResult<ImageAnalysisResult>("Azure Computer Vision configuration is incomplete", System.Net.HttpStatusCode.BadRequest);
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            // Build feature parameters
            var featureParams = new List<string>();
            if (features.HasFlag(ImageAnalysisFeatures.Objects)) featureParams.Add("objects");
            if (features.HasFlag(ImageAnalysisFeatures.Text)) featureParams.Add("read");
            if (features.HasFlag(ImageAnalysisFeatures.Faces)) featureParams.Add("faces");
            if (features.HasFlag(ImageAnalysisFeatures.Labels)) featureParams.Add("tags");
            if (features.HasFlag(ImageAnalysisFeatures.Description)) featureParams.Add("description");

            var requestUrl = $"{endpoint}vision/v3.2/analyze?visualFeatures={string.Join(",", featureParams)}";
            
            using var content = new ByteArrayContent(imageData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            
            var response = await client.PostAsync(requestUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Azure Computer Vision API error: {errorContent}");
                return new ErrorDataResult<ImageAnalysisResult>($"Azure Computer Vision API error: {response.StatusCode}", System.Net.HttpStatusCode.BadRequest);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var analysisResult = ParseAnalysisResult(jsonString, features);

            return new SuccessDataResult<ImageAnalysisResult>(analysisResult, "Image analysis completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            return new ErrorDataResult<ImageAnalysisResult>($"Error analyzing image: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<string>> ExtractTextFromImageAsync(byte[] imageData)
    {
        try
        {
            var analysisResult = await AnalyzeImageAsync(imageData, ImageAnalysisFeatures.Text);
            
            if (!analysisResult.Result)
            {
                return new ErrorDataResult<string>(analysisResult.Message, analysisResult.StatusCode);
            }

            if (string.IsNullOrEmpty(analysisResult.Data.ExtractedText))
            {
                return new SuccessDataResult<string>("No text detected in the image", "Text extraction completed successfully");
            }

            return new SuccessDataResult<string>(analysisResult.Data.ExtractedText, "Text extracted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from image");
            return new ErrorDataResult<string>($"Error extracting text from image: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<string>> GenerateImageDescriptionAsync(byte[] imageData)
    {
        try
        {
            var analysisResult = await AnalyzeImageAsync(imageData, ImageAnalysisFeatures.Description);
            
            if (!analysisResult.Result)
            {
                return new ErrorDataResult<string>(analysisResult.Message, analysisResult.StatusCode);
            }

            if (string.IsNullOrEmpty(analysisResult.Data.Description))
            {
                return new SuccessDataResult<string>("Could not generate a description for this image", "Description generation completed successfully");
            }

            return new SuccessDataResult<string>(analysisResult.Data.Description, "Description generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating image description");
            return new ErrorDataResult<string>($"Error generating image description: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<List<DetectedObject>>> DetectObjectsAsync(byte[] imageData)
    {
        try
        {
            var analysisResult = await AnalyzeImageAsync(imageData, ImageAnalysisFeatures.Objects);
            
            if (!analysisResult.Result)
            {
                return new ErrorDataResult<List<DetectedObject>>(analysisResult.Message, analysisResult.StatusCode);
            }

            if (analysisResult.Data.Objects == null || analysisResult.Data.Objects.Count == 0)
            {
                return new SuccessDataResult<List<DetectedObject>>(new List<DetectedObject>(), "No objects detected in the image");
            }

            return new SuccessDataResult<List<DetectedObject>>(analysisResult.Data.Objects, "Objects detected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting objects in image");
            return new ErrorDataResult<List<DetectedObject>>($"Error detecting objects in image: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }

    private ImageAnalysisResult ParseAnalysisResult(string jsonResult, ImageAnalysisFeatures features)
    {
        var result = new ImageAnalysisResult();
        var responseObj = JsonConvert.DeserializeObject<dynamic>(jsonResult);

        // Parse objects
        if (features.HasFlag(ImageAnalysisFeatures.Objects) && responseObj?.objects != null)
        {
            result.Objects = new List<DetectedObject>();
            foreach (var obj in responseObj.objects)
            {
                var detectedObject = new DetectedObject
                {
                    Name = obj.object?.ToString() ?? "Unknown",
                    Confidence = (float)obj.confidence,
                    Box = new BoundingBox
                    {
                        X = (int)obj.rectangle.x,
                        Y = (int)obj.rectangle.y,
                        Width = (int)obj.rectangle.w,
                        Height = (int)obj.rectangle.h
                    }
                };
                result.Objects.Add(detectedObject);
            }
        }

        // Parse text
        if (features.HasFlag(ImageAnalysisFeatures.Text) && responseObj?.read != null)
        {
            var textBuilder = new StringBuilder();
            foreach (var readResult in responseObj.read.results)
            {
                foreach (var line in readResult.lines)
                {
                    textBuilder.AppendLine(line.text?.ToString() ?? "");
                }
            }
            result.ExtractedText = textBuilder.ToString().Trim();
        }

        // Parse faces
        if (features.HasFlag(ImageAnalysisFeatures.Faces) && responseObj?.faces != null)
        {
            result.Faces = new List<DetectedFace>();
            foreach (var face in responseObj.faces)
            {
                var detectedFace = new DetectedFace
                {
                    Box = new BoundingBox
                    {
                        X = (int)face.faceRectangle.left,
                        Y = (int)face.faceRectangle.top,
                        Width = (int)face.faceRectangle.width,
                        Height = (int)face.faceRectangle.height
                    },
                    Attributes = new Dictionary<string, float>()
                };

                // Add face attributes if available
                if (face.age != null)
                {
                    detectedFace.Attributes["age"] = (float)face.age;
                }
                if (face.gender != null)
                {
                    detectedFace.Attributes["gender"] = face.gender.ToString() == "Male" ? 1.0f : 0.0f;
                }

                result.Faces.Add(detectedFace);
            }
        }

        // Parse labels/tags
        if (features.HasFlag(ImageAnalysisFeatures.Labels) && responseObj?.tags != null)
        {
            result.Labels = new List<ImageLabel>();
            foreach (var tag in responseObj.tags)
            {
                var label = new ImageLabel
                {
                    Name = tag.name?.ToString() ?? "Unknown",
                    Confidence = (float)tag.confidence
                };
                result.Labels.Add(label);
            }
        }

        // Parse description
        if (features.HasFlag(ImageAnalysisFeatures.Description) && responseObj?.description?.captions != null && responseObj.description.captions.Count > 0)
        {
            result.Description = responseObj.description.captions[0].text?.ToString();
        }

        return result;
    }
}