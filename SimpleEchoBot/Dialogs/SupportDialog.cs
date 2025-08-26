using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using SimpleBot.Services;

namespace SimpleBot.Dialogs;


public class SupportDialog
    : ComponentDialog {
    private readonly RuleBasedClassifier _classifier;
    private const string InitialPromptMsg = "Please describe your problem in detail. For example, you can mention what you were trying to do, any error messages you saw, or what seems to be broken.";

    public SupportDialog(RuleBasedClassifier classifier)
        : base(nameof(SupportDialog)) {
        _classifier = classifier;

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
        var promptMessage = MessageFactory.Text(InitialPromptMsg, InitialPromptMsg, InputHints.ExpectingInput);
        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
    }

    private async Task<DialogTurnResult> AnalyzeInputStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {

        var userDescription = (string)stepContext.Result;
        var (mainCategory, entities) = _classifier.AnalyzeText(userDescription);

        stepContext.Values["Category"] = mainCategory;
        stepContext.Values["Entities"] = entities;

        var messageText = $"Okay, I understand you're having a problem with: **{mainCategory}**.";
        if (entities.TryGetValue("ErrorCode", out var errorCode)) {
            messageText += $" I found the error code: `{errorCode}`. This will help us investigate.";
        }
        if (entities.TryGetValue("InvoiceNumber", out var invoiceNumber)) {
            messageText += $" I see your invoice number is `{invoiceNumber}`.";
        }

        messageText += "\n\nIs this correct?";
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
        }
        else {
            // FAILURE: The user did not confirm.
            // You could restart the dialog, transfer to a human agent, or use a different strategy.
            var tryAgainMessage = "I apologize for the mistake. Let's try again. Please describe your problem differently, or you can call our support line directly at 1-800-XXX-XXXX.";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(tryAgainMessage, tryAgainMessage, InputHints.IgnoringInput), cancellationToken);
        }

        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
    }
}
