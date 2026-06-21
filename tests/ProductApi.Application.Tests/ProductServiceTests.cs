using FluentValidation;
using Moq;
using ProductApi.Application.DTOs;
using ProductApi.Application.Interfaces;
using ProductApi.Application.Services;
using ProductApi.Application.Validators;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Exceptions;

namespace ProductApi.Application.Tests;

public sealed class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task CreateProductAsync_WhenRequestIsValid_AddsProductAndReturnsDto()
    {
        var request = new CreateProductRequest
        {
            ProductName = "Laptop",
            Quantity = 5
        };

        Product? addedProduct = null;
        _productRepository
            .Setup(repository => repository.ExistsByNameAsync("Laptop", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _productRepository
            .Setup(repository => repository.Add(It.IsAny<Product>()))
            .Callback<Product>(product =>
            {
                product.Id = 10;
                addedProduct = product;
            });

        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService();

        var result = await service.CreateProductAsync(request, "tester", CancellationToken.None);

        Assert.Equal(10, result.Id);
        Assert.Equal("Laptop", result.ProductName);
        Assert.Equal("tester", result.CreatedBy);
        Assert.Equal(5, result.TotalQuantity);
        Assert.NotNull(addedProduct);
        _productRepository.Verify(repository => repository.Add(It.IsAny<Product>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WhenNameAlreadyExists_ThrowsConflictException()
    {
        var request = new CreateProductRequest
        {
            ProductName = "Laptop",
            Quantity = 5
        };

        _productRepository
            .Setup(repository => repository.ExistsByNameAsync("Laptop", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateProductAsync(request, "tester", CancellationToken.None));
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        _productRepository
            .Setup(repository => repository.GetByIdAsync(99, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetProductByIdAsync(99, CancellationToken.None));
    }

    private ProductService CreateService()
    {
        IValidator<CreateProductRequest> createValidator = new CreateProductRequestValidator();
        IValidator<UpdateProductRequest> updateValidator = new UpdateProductRequestValidator();
        IValidator<ProductQueryParameters> queryValidator = new ProductQueryParametersValidator();

        return new ProductService(
            _productRepository.Object,
            _unitOfWork.Object,
            createValidator,
            updateValidator,
            queryValidator);
    }
}
