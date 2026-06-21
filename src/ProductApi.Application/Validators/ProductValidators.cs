using FluentValidation;
using ProductApi.Application.DTOs;

namespace ProductApi.Application.Validators;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(request => request.ProductName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(request => request.Quantity)
            .GreaterThanOrEqualTo(0)
            .When(request => request.Quantity.HasValue);
    }
}

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(request => request.ProductName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(request => request.Quantity)
            .GreaterThanOrEqualTo(0)
            .When(request => request.Quantity.HasValue);
    }
}

public sealed class ProductQueryParametersValidator : AbstractValidator<ProductQueryParameters>
{
    public ProductQueryParametersValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.Search)
            .MaximumLength(255);
    }
}
