﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.EntityConfigurations
{
    public class PaymentMethodEntityConfiguration : IEntityTypeConfiguration<PaymentMethod>
    {
        public void Configure(EntityTypeBuilder<PaymentMethod> builder)
        {
            builder.ToTable("paymentmethods", OrderDbContext.DEFAULT_SCHEMA);

            builder.HasKey(pm => pm.Id);
            builder.Property(pm => pm.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Ignore(pm => pm.DomainEvents);

            builder.Property<int>("BuyerId")
                   .IsRequired();

            builder.Property(pm => pm.CardHolderName)
                   .UsePropertyAccessMode(PropertyAccessMode.Field)
                   .HasColumnName("CardHolderName")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(pm => pm.Alias)
                   .UsePropertyAccessMode(PropertyAccessMode.Field)
                   .HasColumnName("Alias")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(pm => pm.CardNumber)
                   .UsePropertyAccessMode(PropertyAccessMode.Field)
                   .HasColumnName("CardNumber")
                   .HasMaxLength(25)
                   .IsRequired();

            builder.Property(pm => pm.Expiration)
                   .UsePropertyAccessMode(PropertyAccessMode.Field)
                   .HasColumnName("Expiration")
                   .HasMaxLength(25)
                   .IsRequired();

            builder.Property(pm => pm.CardTypeId)
                   .UsePropertyAccessMode(PropertyAccessMode.Field)
                   .HasColumnName("CardTypeId")
                   .IsRequired();

            builder.HasOne(pm => pm.CardType)
                   .WithMany()
                   .HasForeignKey(pm => pm.Id);
        }
    }
}
