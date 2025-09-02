using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleBot.Infrastructure.Data;

namespace SimpleBot.Infrastructure.Repositories;

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