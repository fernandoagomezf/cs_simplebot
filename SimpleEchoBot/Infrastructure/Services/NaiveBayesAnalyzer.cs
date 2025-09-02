using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SimpleBot.Infrastructure.Repositories;
using SimpleBot.Infrastructure.Services;

public class NaiveBayesAnalyzer
    : ITextAnalyzer {
    private readonly IIntentRepository _repository;
    private readonly Dictionary<string, IntentClassifier> _classifiers;
    private CultureInfo _culture;

    public NaiveBayesAnalyzer(IIntentRepository repository) {
        _repository = repository;
        _culture = Thread.CurrentThread.CurrentCulture;
        _classifiers = new();
    }

    public CultureInfo Culture {
        get => _culture;
        set => _culture = value;
    }

    public async Task<AnalysisResult> AnalyzeAsync(string text) {
        if (String.IsNullOrEmpty(text)) {
            throw new ArgumentException("The text to analyze cannot be empty.");
        }

        var start = DateTime.Now;
        var culture = Culture.Name;
        if (!_classifiers.ContainsKey(culture)) {
            _classifiers[culture] = await CreateClassifierAsync(culture);
        }

        var results = _classifiers[culture].Classify(text);

        return new(text, culture, results, start, DateTime.Now);
    }

    private async Task<IntentClassifier> CreateClassifierAsync(string culture) {
        _repository.UseCulture(culture);
        var intents = await _repository.GetIntentsAsync();
        var trainingData = await _repository.GetTrainingAsync();

        var classifier = new IntentClassifier(intents);
        classifier.Train(trainingData);

        return classifier;
    }
}
