using System;

namespace SimpleBot.Domain;

public record Utterance(
    Guid Id,
    string Text,
    string Culture,
    string Tag
);