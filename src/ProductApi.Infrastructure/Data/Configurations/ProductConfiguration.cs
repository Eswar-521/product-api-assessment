using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApi.Domain.Entities;

namespace ProductApi.Infrastructure.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.ProductName)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(product => product.ProductName)
            .IsUnique();

        builder.Property(product => product.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(product => product.CreatedOn)
            .IsRequired();

        builder.Property(product => product.ModifiedBy)
            .HasMaxLength(100);

        builder.HasMany(product => product.Items)
            .WithOne(item => item.Product)
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
