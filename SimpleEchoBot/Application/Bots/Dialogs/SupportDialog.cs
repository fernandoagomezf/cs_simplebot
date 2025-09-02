using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using SimpleBot.Infrastructure.Services;

namespace SimpleBot.Application.Bots.Dialogs;

public class SupportDialog
    : ComponentDialog {
    private readonly IServiceScopeFactory _services;

    public SupportDialog(IServiceScopeFactory services)
        : base(nameof(SupportDialog)) {
        _services = services;

        var waterfallSteps = new WaterfallStep[] {
            AskQuestionAsync,
            AnalyzeInputStepAsync,
            HandleConfidenceStepAsync,
            ProcessUserSelectionStepAsync,
            FinalStepAsync
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private CultureInfo DetectCulture(string userPrompt) {
        // De momento solo soporta español de México. Idealmente, podríamos detectar
        // el lenguaje del userPrompt y con ello cambiar el idioma (las intenciones
        // soportan cultura así que las intenciones y enunciados se pueden filtrar).
        // Pero eso quedará para una siguiene versión.
        return new CultureInfo("es-MX");
    }

    private async Task<DialogTurnResult> AskQuestionAsync(WaterfallStepContext step, CancellationToken cancellationToken) {
        return await step.PromptAsync(nameof(TextPrompt),
            new PromptOptions {
                Prompt = MessageFactory.Text("Por favor, describe la acción que quieras realizar.")
            },
            cancellationToken
        );
    }

    private async Task<DialogTurnResult> AnalyzeInputStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        using var scope = _services.CreateScope();

        var utterance = (string)stepContext.Result;
        var service = scope.ServiceProvider.GetRequiredService<ITextAnalyzer>();
        service.Culture = DetectCulture(utterance);
        var result = await service.AnalyzeAsync(utterance);

        stepContext.Values["analysisResult"] = result;

        return await stepContext.NextAsync(result, cancellationToken);
    }

    private async Task<DialogTurnResult> HandleConfidenceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        var analysisResult = (AnalysisResult)stepContext.Result;

        if (analysisResult.BestMatch.Confidence > 0.7) {
            // High confidence - skip to final step
            return await stepContext.NextAsync("high_confidence", cancellationToken);
        } else {
            // Low confidence - show options to user
            var topIntents = analysisResult.Classifications.Take(5).ToList();
            stepContext.Values["topIntents"] = topIntents;

            var message = "I'm not completely sure about your request. Which category best describes it?\n\n";
            for (int i = 0; i < topIntents.Count; i++) {
                message += $"{i + 1}. {topIntents[i].IntentName}\n";
            }
            message += "\nPlease select a number (1-5) or type 'none' if none match.";

            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text(message) }, cancellationToken);
        }
    }
    private async Task<DialogTurnResult> ProcessUserSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        var userResponse = (string)stepContext.Result;
        var topIntents = (List<ClassificationResult>)stepContext.Values["topIntents"];
        var originalAnalysis = (AnalysisResult)stepContext.Values["analysisResult"];

        if (int.TryParse(userResponse, out int selection) && selection >= 1 && selection <= topIntents.Count) {
            // User selected one of the options
            var selectedIntent = topIntents[selection - 1];

            // Save to database for training
            /*
            await _trainingService.SaveUtteranceAsync(
                originalAnalysis.OriginalText,
                selectedIntent.Intent
            );
            */

            stepContext.Values["confirmedIntent"] = selectedIntent;
            return await stepContext.NextAsync("user_selected", cancellationToken);
        } else if (userResponse.ToLower() == "none") {
            // Handle case where none match
            stepContext.Values["confirmedIntent"] = null;
            return await stepContext.NextAsync("no_match", cancellationToken);
        } else {
            // Invalid input, reprompt
            var message = "Please select a valid option (1-5) or type 'none':";
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text(message) }, cancellationToken);
        }
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        var outcome = (string)stepContext.Result;

        switch (outcome) {
            case "high_confidence":
                var analysisResult = (AnalysisResult)stepContext.Values["analysisResult"];
                await stepContext.Context.SendActivityAsync(
                    $"I understand you're asking about {analysisResult.BestMatch.IntentName}. Let me help you with that!",
                    cancellationToken: cancellationToken);
                break;

            case "user_selected":
                var selectedIntent = (ClassificationResult)stepContext.Values["confirmedIntent"];
                await stepContext.Context.SendActivityAsync(
                    $"Thank you for clarifying! I'll help you with {selectedIntent.IntentName}.",
                    cancellationToken: cancellationToken);
                break;

            case "no_match":
                await stepContext.Context.SendActivityAsync(
                    "I'll make sure to learn from this. Please try rephrasing your question.",
                    cancellationToken: cancellationToken);
                break;
        }

        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }
}
