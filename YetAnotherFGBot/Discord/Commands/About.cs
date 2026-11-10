using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace YetAnotherFGBot.Discord.Commands
{
    public class About : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("about", "About")]
        public async Task Run()
        {
            var embed = new EmbedBuilder();
            embed.WithColor(Color.Default);
            embed.WithTitle("ABOUT");

            var sb = new StringBuilder();
            embed.WithDescription(sb.ToString());

            await Context.Interaction.FollowupAsync(embed: embed.Build());
        }

    }
}
