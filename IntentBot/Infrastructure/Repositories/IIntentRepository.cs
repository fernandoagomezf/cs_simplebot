using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using IntentBot.Domain;
using IntentBot.Infrastructure.Data;

namespace IntentBot.Infrastructure.Repositories;

public interface IIntentRepository {
    IIntentRepository UseCulture(string culture);
    Task<IEnumerable<Intent>> GetIntentsAsync();
    Task<Intent> GetByIdAsync(Guid id);
    Task<IEnumerable<Intent>> GetByCodeAsync(string code);
    Task<IEnumerable<Utterance>> GetUtterancesAsync();
    Task AddUtteranceAsync(Utterance utterance);
}