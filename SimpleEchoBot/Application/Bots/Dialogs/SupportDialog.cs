using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
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
            DescribeProblemStepAsync,
            AnalyzeInputStepAsync,
            FinalStepAsync,
        };

        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
        AddDialog(new TextPrompt(nameof(TextPrompt)));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> DescribeProblemStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        var msg = "Por favor, describe la acción que quieres realizar.";
        var promptMessage = MessageFactory.Text(msg, msg, InputHints.ExpectingInput);
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
    }

    private async Task<DialogTurnResult> AnalyzeInputStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
        using var scope = _services.CreateScope();
        var analyzer = scope.ServiceProvider.GetRequiredService<ITextAnalyzer>();

        var userDescription = (string)stepContext.Result;
        var result = await analyzer.AnalyzeAsync(userDescription);

        var messageText = String.Empty;
        if (result.HasClassifications) {
            var match = result.BestMatch;
            if (match.IsConfident) {
                messageText = $"Estoy seguro que estás teniendo un problema con **{match.IntentName}** (confidencia: {match.ConfidenceText}%).";
            } else {
                messageText = $"No estoy seguro si te estás refiriendo a **{match.IntentName}** (confidencia: {match.ConfidenceText}%).";
            }
            messageText += "\n\nIs this correct?";
        } else {
            messageText = $"No encontré ninguna acción asociada con el texto que ingresaste :(";
        }

        stepContext.Values["Input"] = userDescription;
        stepContext.Values["Result"] = result;

        var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {

        var userConfirmation = (string)stepContext.Result;
        var category = (string)stepContext.Values["Category"];
        var entities = (Dictionary<string, string>)stepContext.Values["Entities"];

        // Simple check for confirmation ("yes", "y", "correct", etc.)
        if (userConfirmation.ToLowerInvariant().Contains("yes") || userConfirmation.ToLowerInvariant().Contains("y") || userConfirmation.ToLowerInvariant().Contains("correct")) {
            // SUCCESS: The information is confirmed.
            // Here you would:
            // 1. Create the support ticket in your system using the structured data (category, entities)
            // 2. Inform the user.

            var successMessage = $"Perfect! I have created a support ticket for **{category}** and our team will get back to you shortly.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(successMessage, successMessage, InputHints.IgnoringInput), cancellationToken);
        } else {
            // FAILURE: The user did not confirm.
            // You could restart the dialog, transfer to a human agent, or use a different strategy.
            var tryAgainMessage = "I apologize for the mistake. Let's try again. Please describe your problem differently, or you can call our support line directly at 1-800-XXX-XXXX.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(tryAgainMessage, tryAgainMessage, InputHints.IgnoringInput), cancellationToken);
        }

        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }
}
