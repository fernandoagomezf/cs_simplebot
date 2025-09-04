
namespace IntentBot.Infrastructure.Services;

public interface ITextPreprocessor {
    string Preprocess(string text);
    string[] Tokenize(string text);
    string RemoveStopWords(string text);
    string Stem(string text);
    string Normalize(string text);
}