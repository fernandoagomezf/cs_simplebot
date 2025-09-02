using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Identity.Client;
using SimpleBot.Domain;

namespace SimpleBot.Infrastructure.Data;

internal class IntentConfiguration
    : IEntityTypeConfiguration<Intent> {
    public void Configure(EntityTypeBuilder<Intent> builder) {
        builder.ToTable("Intents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired(true);
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired(true);
        builder.Property(x => x.Culture)
            .HasMaxLength(10)
            .IsRequired(true);
        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired(true);
        builder.HasIndex(x => x.Code);
    }
}