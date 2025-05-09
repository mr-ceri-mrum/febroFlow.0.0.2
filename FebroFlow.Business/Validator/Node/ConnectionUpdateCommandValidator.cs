using FluentValidation;
using FebroFlow.Business.UseCase.Node;

namespace FebroFlow.Business.Validator.Node;

public class ConnectionUpdateCommandValidator : AbstractValidator<ConnectionUpdateCommand>
{
    public ConnectionUpdateCommandValidator()
    {
        RuleFor(x => x.Form.Id)
            .NotEmpty().WithMessage("Id is required");
            
        RuleFor(x => x.Form.SourceNodeId)
            .NotEmpty().WithMessage("Source Node Id is required");
            
        RuleFor(x => x.Form.TargetNodeId)
            .NotEmpty().WithMessage("Target Node Id is required")
            .NotEqual(x => x.Form.SourceNodeId).WithMessage("Source and target nodes cannot be the same");
            
        RuleFor(x => x.Form.Type)
            .IsInEnum().WithMessage("Invalid connection type");
    }
}