using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleBot.Domain;

namespace SimpleBot.Infrastructure.Data;

internal class TrainingConfiguration
    : IEntityTypeConfiguration<Training> {
    public void Configure(EntityTypeBuilder<Training> builder) {
        builder.ToTable("Training");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Utterance)
            .IsRequired(true);
        builder.Property(x => x.Culture)
            .HasMaxLength(10)
            .IsRequired(true);
        builder.Property(x => x.Tag)
            .HasMaxLength(50)
            .IsRequired(true);
    }
}