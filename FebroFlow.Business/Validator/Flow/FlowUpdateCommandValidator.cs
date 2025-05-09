using FluentValidation;
using FebroFlow.Business.UseCase.Flow;

namespace FebroFlow.Business.Validator.Flow;

public class FlowUpdateCommandValidator : AbstractValidator<FlowUpdateCommand>
{
    public FlowUpdateCommandValidator()
    {
        RuleFor(x => x.Form.Id)
            .NotEmpty().WithMessage("Id is required");
            
        RuleFor(x => x.Form.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(x => x.Form.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }
}