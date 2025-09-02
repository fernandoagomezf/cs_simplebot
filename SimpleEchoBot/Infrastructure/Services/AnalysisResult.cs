using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleBot.Infrastructure.Services;

public record AnalysisResult(
    string InputText,
    string Culture,
    IEnumerable<ClassificationResult> Classifications,
    DateTime Started,
    DateTime Concluded
) {
    public bool HasClassifications => Classifications.Count() > 0;
    public ClassificationResult BestMatch => Classifications.First();
}