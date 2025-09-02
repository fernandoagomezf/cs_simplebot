using System;

namespace SimpleBot.Domain;

public record Training(
    Guid Id,
    string Utterance,
    string Culture,
    string Tag
);