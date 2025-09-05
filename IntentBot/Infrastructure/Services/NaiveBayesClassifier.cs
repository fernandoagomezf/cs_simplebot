using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using IntentBot.Domain;
using IntentBot.Infrastructure.Repositories;

namespace IntentBot.Infrastructure.Services;

public class NaiveBayesClassifier
    : ITextClassifier {
    private readonly IIntentRepository _repository;
    private readonly ITextPreprocessor _preprocessor;
    private readonly Dictionary<string, IntentClassifier> _classifiers;
    private CultureInfo _culture;

    public NaiveBayesClassifier(IIntentRepository repository, ITextPreprocessor preprocessor) {
        _repository = repository;
        _preprocessor = preprocessor;
        _culture = Thread.CurrentThread.CurrentCulture;
        _classifiers = new();
    }

    public CultureInfo Culture {
        get => _culture;
        set => _culture = value;
    }

    public async Task<ClassificationResult> AnalyzeAsync(string text) {
        if (String.IsNullOrEmpty(text)) {
            throw new ArgumentException("The text to analyze cannot be empty.");
        }

        // esto se añade para poder soportar múltiples culturas. idealmente,
        // el bot podría detectar si el lenguaje es inglés o español o algún
        // otro idioma. dejamos listo esto, pero no está implementado más
        // que para el español.
        var start = DateTime.Now;
        var culture = Culture.Name;
        if (!_classifiers.ContainsKey(culture)) {
            _classifiers[culture] = await CreateClassifierAsync(culture);
        }

        var results = _classifiers[culture].Classify(text);

        return new(text, culture, results, start, DateTime.Now);
    }

    public async Task LearnAsync(string intentCode, string utterance) {
        var item = new Utterance(Guid.NewGuid(), utterance, Culture.Name, intentCode);
        await _repository.AddUtteranceAsync(item);
    }

    private async Task<IntentClassifier> CreateClassifierAsync(string culture) {
        _repository.UseCulture(culture);
        var intents = await _repository.GetIntentsAsync();
        var utterances = await _repository.GetUtterancesAsync();

        var classifier = new IntentClassifier(_preprocessor, intents);
        classifier.Train(utterances);

        return classifier;
    }
}
