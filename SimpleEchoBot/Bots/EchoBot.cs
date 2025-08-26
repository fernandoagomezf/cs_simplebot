
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using SimpleBot.Dialogs;

namespace SimpleBot.Bots;

public class EchoBot
    : ActivityHandler {
    private readonly ConversationState _conversationState;
    private readonly SupportDialog _dialog;

    public EchoBot(ConversationState conversationState, SupportDialog dialog) {
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


        //await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
    }

    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken) {
        foreach (var member in membersAdded) {
            if (member.Id != turnContext.Activity.Recipient.Id) {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome! How can I help you today?"), cancellationToken);

                var dialogSet = new DialogSet(_conversationState.CreateProperty<DialogState>("DialogState"));
                dialogSet.Add(_dialog);
                var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);
                await dialogContext.BeginDialogAsync(_dialog.Id, null, cancellationToken);
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
        }
    }
}

