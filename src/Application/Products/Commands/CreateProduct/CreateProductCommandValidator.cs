using FluentValidation;

namespace Application.Products.Commands.CreateProduct;

internal class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(obj => obj.Name).NotEmpty();
        RuleFor(obj => obj.Description).NotEmpty();
        RuleFor(obj => obj.Price).NotEmpty();
        RuleFor(obj => obj.StockQuantity).GreaterThan(0);
    }
}