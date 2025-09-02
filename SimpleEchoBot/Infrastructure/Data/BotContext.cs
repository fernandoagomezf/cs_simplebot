using System;
using Microsoft.EntityFrameworkCore;
using SimpleBot.Domain;

namespace SimpleBot.Infrastructure.Data;

public class BotContext
    : DbContext {
    public BotContext(DbContextOptions<BotContext> options)
        : base(options) {

    }

    public DbSet<Intent> Intents => Set<Intent>();
    public DbSet<Utterance> Utterances => Set<Utterance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new IntentConfiguration());
        modelBuilder.ApplyConfiguration(new TrainingConfiguration());
    }
}
