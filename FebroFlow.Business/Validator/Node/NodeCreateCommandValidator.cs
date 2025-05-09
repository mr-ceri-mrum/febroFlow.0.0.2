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
            .NotEmpty().WithMessage("Flow ID is required");
    }
}