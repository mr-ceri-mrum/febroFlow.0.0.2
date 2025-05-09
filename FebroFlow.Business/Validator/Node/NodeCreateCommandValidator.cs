using FluentValidation;
using FebroFlow.Business.UseCase.Node;

namespace FebroFlow.Business.Validator.Node;

public class NodeCreateCommandValidator : AbstractValidator<NodeCreateCommand>
{
    public NodeCreateCommandValidator()
    {
        RuleFor(x => x.Form.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(x => x.Form.Type)
            .IsInEnum().WithMessage("Invalid node type");
            
        RuleFor(x => x.Form.FlowId)
            .NotEmpty().WithMessage("Flow Id is required");
            
        RuleFor(x => x.Form.Parameters)
            .Must(BeValidJson).WithMessage("Parameters must be valid JSON");
    }
    
    private bool BeValidJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return true; // Empty is valid
        }
        
        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}