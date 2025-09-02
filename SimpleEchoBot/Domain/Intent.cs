
using System;

namespace SimpleBot.Domain;

public record Intent(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string Culture
);