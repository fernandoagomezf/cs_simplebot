using System;
using System.Linq;
using System.Collections.Generic;
using SimpleBot.Domain;

namespace SimpleBot.Infrastructure.Services;

public class IntentClassifier {
    private readonly ITextPreprocessor _preprocessor;
    private readonly Dictionary<string, Intent> _intents;
    private readonly Dictionary<string, List<string>> _trainingTexts = new();
    private readonly Dictionary<string, Dictionary<string, int>> _wordFrequencies = new();
    private readonly Dictionary<string, int> _intentWordCounts = new();

    internal IntentClassifier(ITextPreprocessor preprocessor, IEnumerable<Intent> intents) {
        _preprocessor = preprocessor;
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

    public void Train(IEnumerable<Utterance> training) {
        foreach (var item in training) {
            if (_trainingTexts.TryGetValue(item.Tag, out var texts)) {
                texts.Add(item.Text);
                var preprocText = _preprocessor.Preprocess(item.Text);
                var words = _preprocessor.Tokenize(preprocText);
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

    public IEnumerable<IntentResult> Classify(string text) {
        var preprocText = _preprocessor.Preprocess(text);
        var words = _preprocessor.Tokenize(preprocText);
        var scores = new Dictionary<string, double>();

        foreach (var intentCode in _intents.Keys) {
            var intentCodeCount = (double)_trainingTexts[intentCode].Count;
            var trainingTextSum = (double)_trainingTexts.Values.Sum(t => t.Count);
            var ratio = intentCodeCount / trainingTextSum;
            var intentProbability = Math.Log(ratio);

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
            .Select(kvp => new IntentResult(
                IntentId: _intents[kvp.Key].Id,
                IntentCode: kvp.Key,
                IntentName: _intents[kvp.Key].Name,
                Confidence: kvp.Value / sumExpScores
            ))
            .OrderByDescending(r => r.Confidence)
            .ToList();
    }
}