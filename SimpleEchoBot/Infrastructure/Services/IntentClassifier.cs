using System;
using System.Linq;
using System.Collections.Generic;
using SimpleBot.Domain;

namespace SimpleBot.Infrastructure.Services;

public class IntentClassifier {
    private readonly Dictionary<string, Intent> _intents;
    private readonly Dictionary<string, List<string>> _trainingTexts = new();
    private readonly Dictionary<string, Dictionary<string, int>> _wordFrequencies = new();
    private readonly Dictionary<string, int> _intentWordCounts = new();

    internal IntentClassifier(IEnumerable<Intent> intents) {
        _intents = new();
        _trainingTexts = new();
        _wordFrequencies = new();
        _intentWordCounts = new();

        foreach (var intent in intents) {
            _intents[intent.Code] = intent;
            _trainingTexts[intent.Code] = new();
            _wordFrequencies[intent.Code] = new();
            _intentWordCounts[intent.Code] = 0;
        }
    }

    private static IEnumerable<string> Tokenize(string text) {
        return text.ToLowerInvariant()
            .Split([' ', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}'],
                StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 2) // Ignore very short words
            .Distinct();
    }

    public void Train(IEnumerable<Training> training) {
        foreach (var item in training) {
            if (_trainingTexts.TryGetValue(item.Tag, out var texts)) {
                texts.Add(item.Text);
                var words = Tokenize(item.Text);
                foreach (var word in words) {
                    if (!_wordFrequencies[item.Tag].TryGetValue(word, out var count)) {
                        count = 0;
                    }
                    _wordFrequencies[item.Tag][word] = count + 1;
                    _intentWordCounts[item.Tag]++;
                }
            }
        }
    }

    public IEnumerable<ClassificationResult> Classify(string text) {
        var words = Tokenize(text);
        var scores = new Dictionary<string, double>();

        foreach (var intentCode in _intents.Keys) {
            var intentProbability = Math.Log((double)_trainingTexts[intentCode].Count /
                                           _trainingTexts.Values.Sum(t => t.Count));

            var wordProbability = 0.0;
            foreach (var word in words) {
                var wordCount = _wordFrequencies[intentCode].TryGetValue(word, out var count) ? count : 0;
                var totalWords = _intentWordCounts[intentCode];

                // Laplace smoothing to avoid zero probabilities
                var probability = (double)(wordCount + 1) / (totalWords + _wordFrequencies[intentCode].Count + 1);
                wordProbability += Math.Log(probability);
            }

            scores[intentCode] = intentProbability + wordProbability;
        }

        // Convert to probabilities and normalize
        var maxScore = scores.Values.Max();
        var expScores = scores.ToDictionary(
            kvp => kvp.Key,
            kvp => Math.Exp(kvp.Value - maxScore) // Avoid underflow
        );

        var sumExpScores = expScores.Values.Sum();

        return expScores
            .Select(kvp => new ClassificationResult(
                IntentId: _intents[kvp.Key].Id,
                IntentCode: kvp.Key,
                IntentName: _intents[kvp.Key].Name,
                Confidence: kvp.Value / sumExpScores
            ))
            .OrderByDescending(r => r.Confidence)
            .ToList();
    }
}