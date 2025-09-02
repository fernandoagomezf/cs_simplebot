using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using SimpleBot.Domain;
using SimpleBot.Infrastructure.Data;

namespace SimpleBot.Infrastructure.Repositories;

public interface IIntentRepository {
    IIntentRepository UseCulture(string culture);
    Task<IEnumerable<Intent>> GetIntentsAsync();
    Task<Intent> GetByIdAsync(Guid id);
    Task<IEnumerable<Intent>> GetByCodeAsync(string code);
    Task<IEnumerable<Utterance>> GetUtterancesAsync();
}