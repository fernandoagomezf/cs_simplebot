using System;

namespace SimpleBot.Domain;

public record Training(
    Guid Id,
    string Text,
    string Culture,
    string Tag
);