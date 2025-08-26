
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace SimpleBot.Adapters;

public class AdapterWithErrorHandler
    : CloudAdapter {
    public AdapterWithErrorHandler(BotFrameworkAuthentication auth, ILogger<CloudAdapter> logger, ConversationState conversationState = null)
           : base(auth, logger) {
        OnTurnError = async (turnContext, exception) => {
            // Log any leaked exception from the application
            logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

            // Send a message to the user
            await turnContext.SendActivityAsync("The bot encountered an error or bug.");
            await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");

            // Clear the conversation state if available to prevent infinite loops
            if (conversationState != null) {
                try {
                    await conversationState.DeleteAsync(turnContext);
                }
                catch (Exception ex) {
                    logger.LogError(ex, $"Exception caught on attempting to Delete ConversationState : {ex.Message}");
                }
            }
        };
    }
}

