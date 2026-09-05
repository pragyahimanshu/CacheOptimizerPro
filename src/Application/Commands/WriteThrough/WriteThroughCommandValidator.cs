using Application.Commands.WriteThrough;
using FluentValidation;

namespace Application.Commands.WriteThrough;

internal sealed class WriteThroughCommandValidator 
    : AbstractValidator<WriteThroughCommand>
{
    public WriteThroughCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Cache key is required.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value cannot be empty.");

        RuleFor(x => x.AbsoluteExpiration)
            .GreaterThan(TimeSpan.Zero).WithMessage("Absolute expiration must be positive.");

        RuleFor(x => x.UpdateDataSource)
            .NotNull().WithMessage("Update data source is required.");
    }
}