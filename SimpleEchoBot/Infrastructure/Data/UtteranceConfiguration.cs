using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SimpleBot.Domain;

namespace SimpleBot.Infrastructure.Data;

internal class TrainingConfiguration
    : IEntityTypeConfiguration<Utterance> {
    public void Configure(EntityTypeBuilder<Utterance> builder) {
        builder.ToTable("Utterances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text)
            .IsRequired(true);
        builder.Property(x => x.Culture)
            .HasMaxLength(10)
            .IsRequired(true);
        builder.Property(x => x.Tag)
            .HasMaxLength(50)
            .IsRequired(true);
    }
}