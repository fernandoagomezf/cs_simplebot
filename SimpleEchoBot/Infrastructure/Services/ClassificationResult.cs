using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleBot.Infrastructure.Services;

public record ClassificationResult(
    string InputText,
    string Culture,
    IEnumerable<IntentResult> Intents,
    DateTime Started,
    DateTime Concluded
) {
    public bool HasIntents => Intents.Count() > 0;
    public IntentResult BestMatch => Intents.First();
}