using ProductApi.Application.DTOs;

namespace ProductApi.Application.Interfaces;

public interface IProductService
{
    Task<PagedResponse<ProductDto>> GetProductsAsync(ProductQueryParameters query, CancellationToken cancellationToken);
    Task<ProductDto> GetProductByIdAsync(int id, CancellationToken cancellationToken);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request, string createdBy, CancellationToken cancellationToken);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductRequest request, string modifiedBy, CancellationToken cancellationToken);
    Task DeleteProductAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemDto>> GetProductItemsAsync(int productId, CancellationToken cancellationToken);
}
