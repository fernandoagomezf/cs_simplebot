using System;

namespace IntentBot.Infrastructure.Services;

public record IntentResult(
    Guid IntentId,
    string IntentCode,
    string IntentName,
    double Confidence
) {
    public const double Threshold = 0.7; // 70% de probabilidad para arriba lo consideramos confidente.
    public bool IsConfident => Confidence >= Threshold;
    public string ConfidenceText => $"{Confidence:P1}";
}
