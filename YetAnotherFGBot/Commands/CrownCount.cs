using Discord;
using Discord.Interactions;
using Discord.Net;
using YetAnotherFGBot.ModuleExtension;

namespace YetAnotherFGBot.Commands
{
    public class CrownCount : FGGatewayModule
    {
        [SlashCommand("crown-count", "See your current Fall Guys crown count")]
        public async Task Ping()
        {
            await DeferAsync(true);
            if (!await RequestFGLogin()) return;

            var embed = new EmbedBuilder();
            embed.WithColor(Color.Orange);
            embed.WithTitle("CROWN COUNT");
            embed.WithDescription($"Logged in as ``{EOSName}``, pulling crown count data...");
            await Context.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true);


        }
    }
}
