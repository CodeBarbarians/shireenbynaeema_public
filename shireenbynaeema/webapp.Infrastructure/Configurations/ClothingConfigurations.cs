namespace Infrastructure.Configurations
{
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DeliveryItemConfiguration : IEntityTypeConfiguration<DeliveryItem>
{
    public void Configure(EntityTypeBuilder<DeliveryItem> builder)
    {
        builder.HasOne(d => d.Delivery).WithMany().HasForeignKey(d => d.DeliveryId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(d => d.OrderItem).WithMany().HasForeignKey(d => d.OrderItemId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class ReturnItemConfiguration : IEntityTypeConfiguration<ReturnItem>
{
    public void Configure(EntityTypeBuilder<ReturnItem> builder)
    {
        builder.HasOne(r => r.Return).WithMany().HasForeignKey(r => r.ReturnId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(r => r.OrderItem).WithMany().HasForeignKey(r => r.OrderItemId).OnDelete(DeleteBehavior.NoAction);
    }
}
}
