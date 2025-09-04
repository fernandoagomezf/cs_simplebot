
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using IntentBot.Application.Bots.Dialogs;

namespace IntentBot.Application.Bots;

public class IntentDetectionBot
    : ActivityHandler {
    private readonly ConversationState _conversationState;
    private readonly IntentDialog _dialog;

    public IntentDetectionBot(ConversationState conversationState, IntentDialog dialog) {
        _conversationState = conversationState;
        _dialog = dialog;
    }

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken) {

        var dialogSet = new DialogSet(_conversationState.CreateProperty<DialogState>("DialogState"));
        dialogSet.Add(_dialog);
        var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);

        var result = await dialogContext.ContinueDialogAsync(cancellationToken);
        if (result.Status == DialogTurnStatus.Empty || result.Status == DialogTurnStatus.Complete) {
            if (result.Status == DialogTurnStatus.Complete) {
                await dialogContext.CancelAllDialogsAsync(cancellationToken);
            }

            await dialogContext.BeginDialogAsync(_dialog.Id, null, cancellationToken);
        }

        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken) {
        foreach (var member in membersAdded) {
            if (member.Id != turnContext.Activity.Recipient.Id) {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Bienvenido al BlendBot empresarial, v0.1"), cancellationToken);

                var dialogSet = new DialogSet(_conversationState.CreateProperty<DialogState>("DialogState"));
                dialogSet.Add(_dialog);
                var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);
                await dialogContext.BeginDialogAsync(_dialog.Id, null, cancellationToken);
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
        }
    }
}

