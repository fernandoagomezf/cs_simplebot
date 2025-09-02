
using System.Globalization;
using System.Threading.Tasks;

namespace SimpleBot.Infrastructure.Services;

public interface ITextAnalyzer {
    CultureInfo Culture { get; set; }
    Task<AnalysisResult> AnalyzeAsync(string text);
}