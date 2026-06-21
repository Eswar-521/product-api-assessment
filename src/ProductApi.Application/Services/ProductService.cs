using FluentValidation;
using ProductApi.Application.DTOs;
using ProductApi.Application.Interfaces;
using ProductApi.Application.Mapping;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Exceptions;

namespace ProductApi.Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly IValidator<ProductQueryParameters> _queryValidator;

    public ProductService(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator,
        IValidator<ProductQueryParameters> queryValidator)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _queryValidator = queryValidator;
    }

    public async Task<PagedResponse<ProductDto>> GetProductsAsync(ProductQueryParameters query, CancellationToken cancellationToken)
    {
        await ValidateAsync(_queryValidator, query, cancellationToken);

        var (products, totalCount) = await _productRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            query.Search,
            cancellationToken);

        return new PagedResponse<ProductDto>
        {
            Items = products.Select(product => product.ToDto()).ToArray(),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDto> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, trackChanges: false, cancellationToken)
            ?? throw new NotFoundException($"Product with id '{id}' was not found.");

        return product.ToDto();
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, string createdBy, CancellationToken cancellationToken)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);

        if (await _productRepository.ExistsByNameAsync(request.ProductName, null, cancellationToken))
        {
            throw new ConflictException($"Product '{request.ProductName}' already exists.");
        }

        var product = new Product
        {
            ProductName = request.ProductName.Trim(),
            CreatedBy = createdBy,
            CreatedOn = DateTime.UtcNow
        };

        if (request.Quantity.HasValue)
        {
            product.Items.Add(new Item { Quantity = request.Quantity.Value });
        }

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductRequest request, string modifiedBy, CancellationToken cancellationToken)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);

        var product = await _productRepository.GetByIdAsync(id, trackChanges: true, cancellationToken)
            ?? throw new NotFoundException($"Product with id '{id}' was not found.");

        if (await _productRepository.ExistsByNameAsync(request.ProductName, id, cancellationToken))
        {
            throw new ConflictException($"Product '{request.ProductName}' already exists.");
        }

        product.ProductName = request.ProductName.Trim();
        product.ModifiedBy = modifiedBy;
        product.ModifiedOn = DateTime.UtcNow;

        if (request.Quantity.HasValue)
        {
            var item = product.Items.FirstOrDefault();
            if (item is null)
            {
                product.Items.Add(new Item { Quantity = request.Quantity.Value });
            }
            else
            {
                item.Quantity = request.Quantity.Value;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.ToDto();
    }

    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(id, trackChanges: true, cancellationToken)
            ?? throw new NotFoundException($"Product with id '{id}' was not found.");

        _productRepository.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ItemDto>> GetProductItemsAsync(int productId, CancellationToken cancellationToken)
    {
        var productExists = await _productRepository.GetByIdAsync(productId, trackChanges: false, cancellationToken) is not null;
        if (!productExists)
        {
            throw new NotFoundException($"Product with id '{productId}' was not found.");
        }

        var items = await _productRepository.GetItemsAsync(productId, cancellationToken);

        return items.Select(item => item.ToDto()).ToArray();
    }

    private static async Task ValidateAsync<T>(IValidator<T> validator, T request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}
