namespace ProductApi.Application.DTOs;

public sealed class ProductDto
{
    public int Id { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedOn { get; init; }
    public string? ModifiedBy { get; init; }
    public DateTime? ModifiedOn { get; init; }
    public int TotalQuantity { get; init; }
    public IReadOnlyCollection<ItemDto> Items { get; init; } = Array.Empty<ItemDto>();
}

public sealed class ItemDto
{
    public int Id { get; init; }
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}

public sealed class CreateProductRequest
{
    public string ProductName { get; init; } = string.Empty;
    public int? Quantity { get; init; }
}

public sealed class UpdateProductRequest
{
    public string ProductName { get; init; } = string.Empty;
    public int? Quantity { get; init; }
}
