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
        var classifier = scope.ServiceProvider.GetRequiredService<ITextClassifier>();
        classifier.Culture = DetectCulture(utterance);
        var result = await classifier.AnalyzeAsync(utterance);

        stepContext.Values["analysisResult"] = result;

        return await stepContext.NextAsync(result, cancellationToken);
    }

    private async Task<DialogTurnResult> HandleConfidenceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        var analysisResult = (ClassificationResult)stepContext.Result;

        if (analysisResult.BestMatch.Confidence > 0.7) {
            stepContext.Values["askClarifiction"] = false;
            return await stepContext.NextAsync("high_confidence", cancellationToken);
        } else {
            stepContext.Values["askClarifiction"] = true;
            var topIntents = analysisResult.Intents.Take(5).ToList();
            stepContext.Values["topIntents"] = topIntents;

            var message = "No estoy completamente seguro de entender tu solicitud. ¿Cuál de estas opciones lo describe mejor?\n\n";
            for (int i = 0; i < topIntents.Count; i++) {
                message += $"{i + 1}. {topIntents[i].IntentName} (confidencia: {topIntents[i].ConfidenceText})\n";
            }
            message += "\nPor favor, selecciona una opción (No. 1-5) o escribe 'ninguna' si no concuerda.";

            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text(message) }, cancellationToken);
        }
    }
    private async Task<DialogTurnResult> ProcessUserSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        var clarificationNeeded = (bool)stepContext.Values["askClarifiction"];
        if (clarificationNeeded) {
            var userResponse = (string)stepContext.Result;
            var topIntents = (List<IntentResult>)stepContext.Values["topIntents"];
            var originalAnalysis = (ClassificationResult)stepContext.Values["analysisResult"];

            if (int.TryParse(userResponse, out int selection) && selection >= 1 && selection <= topIntents.Count) {
                var selectedIntent = topIntents[selection - 1];

                using var scope = _services.CreateScope();
                var classifier = scope.ServiceProvider.GetRequiredService<ITextClassifier>();
                classifier.Culture = DetectCulture(originalAnalysis.InputText);
                await classifier.LearnAsync(selectedIntent.IntentCode, originalAnalysis.InputText);

                stepContext.Values["confirmedIntent"] = selectedIntent;
                return await stepContext.NextAsync("user_selected", cancellationToken);
            } else if (userResponse.ToLower() == "no" || userResponse.ToLower() == "ninguno" || userResponse.ToLower() == "ninguna" || userResponse.ToLower() == "nada") {
                // TODO: hacer algo si no se encuentra nada.
                stepContext.Values["confirmedIntent"] = null;
                return await stepContext.NextAsync("no_match", cancellationToken);
            } else {
                var message = "\nPor favor, selecciona una opción (No. 1-5) o escribe 'ninguna' si no concuerda.";
                return await stepContext.PromptAsync(nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text(message) }, cancellationToken);
            }
        } else {
            return await stepContext.NextAsync(stepContext.Result, cancellationToken);
        }
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        var outcome = (string)stepContext.Result;

        switch (outcome) {
            case "high_confidence":
                var analysisResult = (ClassificationResult)stepContext.Values["analysisResult"];
                await stepContext.Context.SendActivityAsync(
                    $"Entiendo que la acción que quieres realizar es **{analysisResult.BestMatch.IntentName}** (confidencia: {analysisResult.BestMatch.ConfidenceText}).",
                    cancellationToken: cancellationToken);
                break;

            case "user_selected":
                var selectedIntent = (IntentResult)stepContext.Values["confirmedIntent"];
                await stepContext.Context.SendActivityAsync(
                    $"¡Gracias por clarificar! Ahora entiendo que la acción es **{selectedIntent.IntentName}**.",
                    cancellationToken: cancellationToken);
                break;

            case "no_match":
                await stepContext.Context.SendActivityAsync(
                    "Lo siento, intentaré aprender de esto. Por favor, refrasea tu pregunta e intenta de nuevo.",
                    cancellationToken: cancellationToken);
                break;
        }

        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }
}
