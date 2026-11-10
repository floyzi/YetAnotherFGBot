using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace YetAnotherFGBot.Commands
{
    public class BaseModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("crown-count", "Crown Count")]
        public async Task Ping()
        {
            var loginProcessor = new FGLoginProcessor(Context.Interaction);

            await DeferAsync(true);

            await loginProcessor.RequestEOSLogin();
        }
    }
}
