using System;

namespace IntentBot.Domain;

public record Utterance(
    Guid Id,
    string Text,
    string Culture,
    string Tag
);