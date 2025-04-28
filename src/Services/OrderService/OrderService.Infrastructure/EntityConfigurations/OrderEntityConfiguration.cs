using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.AggregateModels.OrderAggregate;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.EntityConfigurations
{
    public class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders", OrderDbContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedOnAdd();

            builder.Ignore(o => o.DomainEvents);

            builder.OwnsOne(o => o.Address, a =>
            {
                a.WithOwner();
            });

            builder.Property<int>("OrderStatusId")
                   .UsePropertyAccessMode(PropertyAccessMode.Field)
                   .HasColumnName("OrderStatusId")
                   .IsRequired();

            var navigation = builder.Metadata.FindNavigation(nameof(Order.OrderItems));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne(o => o.Buyer)
                   .WithMany()
                   .HasForeignKey(o => o.Buyer);

            builder.HasOne(o => o.OrderStatus)
                   .WithMany()
                   .HasForeignKey("OrderStatusId");
        }
    }
}
