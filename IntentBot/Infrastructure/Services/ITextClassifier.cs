
using System.Globalization;
using System.Threading.Tasks;

namespace IntentBot.Infrastructure.Services;

public interface ITextClassifier {
    CultureInfo Culture { get; set; }
    Task<ClassificationResult> AnalyzeAsync(string text);
    Task LearnAsync(string intentCode, string utterance);
}