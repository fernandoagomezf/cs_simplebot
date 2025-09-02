
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleBot.Domain;
using SimpleBot.Infrastructure.Data;

namespace SimpleBot.Infrastructure.Repositories;

public sealed class IntentRepository
    : Repository, IIntentRepository {
    private string _culture;

    public IntentRepository(BotContext context)
        : base(context) {
        _culture = String.Empty;
    }

    public async Task<IEnumerable<Intent>> GetByCodeAsync(string code)
        => await Context.Set<Intent>()
            .Where(x => x.Code == code)
            .Where(x => x.Culture == _culture)
            .ToListAsync();

    public async Task<Intent> GetByIdAsync(Guid id) {
        var item = await Context.Set<Intent>()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
        if (item is null) {
            throw new ArgumentException($"Intent with ID {id} not found.");
        }

        return item;
    }

    public async Task<IEnumerable<Intent>> GetIntentsAsync()
        => await Context.Set<Intent>()
            .Where(x => x.Culture == _culture)
            .OrderBy(x => x.Code)
            .ToListAsync();

    public async Task<IEnumerable<Training>> GetTrainingAsync()
        => await Context.Set<Training>()
            .Where(x => x.Culture == _culture)
            .OrderBy(x => x.Tag)
            .ToListAsync();

    public IIntentRepository UseCulture(string culture) {
        var isValid = CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Any(x => String.Equals(x.Name, culture, StringComparison.OrdinalIgnoreCase));
        if (!isValid) {
            throw new ArgumentException($"The culture {culture} is not valid.");
        }
        _culture = culture;

        return this;
    }
}