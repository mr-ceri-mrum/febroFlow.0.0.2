using FluentValidation;
using FebroFlow.Business.UseCase.Node;

namespace FebroFlow.Business.Validator.Node;

public class ConnectionCreateCommandValidator : AbstractValidator<ConnectionCreateCommand>
{
    public ConnectionCreateCommandValidator()
    {
        RuleFor(x => x.ConnectionDto.SourceNodeId)
            .NotEmpty().WithMessage("Source Node Id is required");
            
        RuleFor(x => x.ConnectionDto.TargetNodeId)
            .NotEmpty().WithMessage("Target Node Id is required")
            .NotEqual(x => x.ConnectionDto.SourceNodeId).WithMessage("Source and target nodes cannot be the same");
            
        RuleFor(x => x.ConnectionDto.Type)
            .IsInEnum().WithMessage("Invalid connection type");
            
        RuleFor(x => x.ConnectionDto.FlowId)
            .NotEmpty().WithMessage("Flow Id is required");
    }
}