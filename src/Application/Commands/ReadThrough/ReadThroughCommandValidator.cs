using Application.Commands.ReadThrough;
using FluentValidation;

namespace Application.Commands.ReadThrough;

internal sealed class ReadThroughCommandValidator 
    : AbstractValidator<ReadThroughCommand>
{
    public ReadThroughCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Cache key is required.");

        RuleFor(x => x.FallbackDataSource)
            .NotNull().WithMessage("Fallback data source is required.");

        RuleFor(x => x.AbsoluteExpiration)
            .GreaterThan(TimeSpan.Zero).WithMessage("Absolute expiration must be positive.");
    }
}