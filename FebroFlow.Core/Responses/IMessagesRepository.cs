using Microsoft.Extensions.Configuration;

namespace FebroFlow.Core.Responses;

public interface IMessagesRepository
    {
        string AccessDenied(string data);
        string FormValidation();
        string Deleted();
        string Created(string data);
        string Edited(string data);
        string NotEmpty(string data);
        string ShouldBeUnique(string data);
        string ValidValueInEnum(string data, string enumName);
        string NotFound();
        string NotFound(string data);
        string NotEqual(string data, string equalData);
        string CantDelete(string reason);
        string Response200();
    }

    public class MessagesRepository(IConfiguration configuration) : IMessagesRepository
    {
        public string AccessDenied(string data) =>
        configuration.GetSection("ResponseMessages").GetSection("AccessDenied").Value!.Replace("{{NAME}}", data);

        public string FormValidation() =>
            configuration.GetSection("ResponseMessages").GetSection("FormValidation").Value!;

        public string Deleted() =>
            configuration.GetSection("ResponseMessages").GetSection("Deleted").Value!;

        public string Created(string data) =>
            configuration.GetSection("ResponseMessages").GetSection("Created").Value!.Replace("{{NAME}}", data);

        public string Edited(string data) =>
            configuration.GetSection("ResponseMessages").GetSection("Edited").Value!.Replace("{{NAME}}", data);

        public string NotEmpty(string data) =>
            configuration.GetSection("ResponseMessages").GetSection("NotEmpty").Value!.Replace("{{PROPERTYNAME}}", data);

        public string ShouldBeUnique(string data) =>
            configuration.GetSection("ResponseMessages").GetSection("ShouldBeUnique").Value!.Replace("{{PROPERTYNAME}}",
                data);

        public string ValidValueInEnum(string data, string enumName) =>
            configuration.GetSection("ResponseMessages").GetSection("ValidValueInEnum").Value!
                .Replace("{{PROPERTYNAME}}", data).Replace("{{ENUMNAME}}", enumName);

        public string NotFound() => configuration.GetSection("ResponseMessages").GetSection("NotFound").Value!;

        public string NotEqual(string data, string equalData) =>
            configuration.GetSection("ResponseMessages").GetSection("NotEqual").Value!
                .Replace("{{PROPERTYNAME}}", data).Replace("{{EQUALDATA}}", equalData);

        public string CantDelete(string reason) =>
            configuration.GetSection("ResponseMessages").GetSection("CantDelete").Value!
                .Replace("{{REASON}}", reason);

        public string Response200()
        {
            return configuration.GetSection("ResponseMessages").GetSection("Success").Value!;
        }

        public string NotFound(string data) =>
            configuration.GetSection("ResponseMessages").GetSection("NotFoundWithMessage").Value!
                .Replace("{{PROPERTYNAME}}", data);
    }