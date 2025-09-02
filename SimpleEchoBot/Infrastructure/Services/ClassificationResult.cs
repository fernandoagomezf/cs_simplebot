using System;

namespace SimpleBot.Infrastructure.Services;

public record ClassificationResult(
    Guid IntentId,
    string IntentCode,
    string IntentName,
    double Confidence
) {
    public const double Threshold = 0.7;
    public bool IsConfident => Confidence >= Threshold;
    public string ConfidenceText => $"{Confidence:P1}";
}
