
using System;

namespace IntentBot.Domain;

public record Intent(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string Culture
);