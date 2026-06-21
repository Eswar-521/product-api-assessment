using Microsoft.EntityFrameworkCore;
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;
using ProductApi.Infrastructure.Data;

namespace ProductApi.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<Product> Products, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Products
            .Include(product => product.Items)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(product => product.ProductName.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderBy(product => product.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products, totalCount);
    }

    public Task<Product?> GetByIdAsync(int id, bool trackChanges, CancellationToken cancellationToken)
    {
        var query = _dbContext.Products
            .Include(product => product.Items)
            .AsQueryable();

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Item>> GetItemsAsync(int productId, CancellationToken cancellationToken)
    {
        return await _dbContext.Items
            .Where(item => item.ProductId == productId)
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string productName, int? excludedProductId, CancellationToken cancellationToken)
    {
        var normalizedName = productName.Trim();

        return _dbContext.Products.AnyAsync(
            product => product.ProductName == normalizedName
                && (!excludedProductId.HasValue || product.Id != excludedProductId.Value),
            cancellationToken);
    }

    public void Add(Product product)
    {
        _dbContext.Products.Add(product);
    }

    public void Remove(Product product)
    {
        _dbContext.Products.Remove(product);
    }
}
