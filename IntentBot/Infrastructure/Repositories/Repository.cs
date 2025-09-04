using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IntentBot.Infrastructure.Data;

namespace IntentBot.Infrastructure.Repositories;

public abstract class Repository {
    private readonly BotContext _context;

    protected Repository(BotContext context) {
        _context = context;
    }

    protected BotContext Context => _context;

    public async Task UpdateAsync() {
        await Context.SaveChangesAsync();
    }
}