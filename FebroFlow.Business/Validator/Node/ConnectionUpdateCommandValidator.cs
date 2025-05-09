using FluentValidation;
using FebroFlow.Business.UseCase.Node;

namespace FebroFlow.Business.Validator.Node;

public class ConnectionUpdateCommandValidator : AbstractValidator<ConnectionUpdateCommand>
{
    public ConnectionUpdateCommandValidator()
    {
        RuleFor(x => x.Form.Id)
            .NotEmpty().WithMessage("Connection ID is required");
            
        RuleFor(x => x.Form.SourceNodeId)
            .NotEmpty().WithMessage("Source node ID is required");
            
        RuleFor(x => x.Form.TargetNodeId)
            .NotEmpty().WithMessage("Target node ID is required")
            .NotEqual(x => x.Form.SourceNodeId).WithMessage("Source and target nodes must be different");
            
        RuleFor(x => x.Form.Type)
            .IsInEnum().WithMessage("Invalid connection type");
    }
}