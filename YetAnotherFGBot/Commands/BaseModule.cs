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
            await RespondAsync($"Pong! {Context.Client.Latency}ms", ephemeral: true);
        }
    }
}
