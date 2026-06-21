using ProductApi.Application.DTOs;
using ProductApi.Domain.Entities;

namespace ProductApi.Application.Mapping;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product)
    {
        var items = product.Items
            .Select(item => item.ToDto())
            .ToArray();

        return new ProductDto
        {
            Id = product.Id,
            ProductName = product.ProductName,
            CreatedBy = product.CreatedBy,
            CreatedOn = product.CreatedOn,
            ModifiedBy = product.ModifiedBy,
            ModifiedOn = product.ModifiedOn,
            TotalQuantity = items.Sum(item => item.Quantity),
            Items = items
        };
    }

    public static ItemDto ToDto(this Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            Quantity = item.Quantity
        };
    }
}
