using ProductApi.Domain.Entities;

namespace ProductApi.Application.Interfaces;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> Products, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(int id, bool trackChanges, CancellationToken cancellationToken);
    Task<IReadOnlyList<Item>> GetItemsAsync(int productId, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string productName, int? excludedProductId, CancellationToken cancellationToken);
    void Add(Product product);
    void Remove(Product product);
}
