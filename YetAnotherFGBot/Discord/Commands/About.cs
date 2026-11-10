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
            var upT = DateTime.UtcNow - Program.Startup;

            var embed = new EmbedBuilder();
            embed.WithColor(Color.LightGrey);
            embed.WithTitle("ABOUT");
            embed.WithThumbnailUrl(Emotes.Credits.Url);

            var sb = new StringBuilder();
            sb.AppendLine("## Yet another Fall Guys bot");
            sb.AppendLine("Created by [floyzi](https://github.com/floyzi/)");
            sb.AppendLine($"Version V{YetAnotherFGBotBuildDetails.Version} (#{YetAnotherFGBotBuildDetails.BuildNumber})");
            sb.AppendLine();
            sb.AppendLine($"- Build Date: {TimestampTag.FromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(YetAnotherFGBotBuildDetails.BuildTimestamp), TimestampTagStyles.LongDateTime)}");
            sb.AppendLine($"- Uptime: ``{(int)upT.TotalDays}d {upT.Hours}h {upT.Minutes}m {upT.Seconds}s`` ({TimestampTag.FromDateTime(Program.Startup)})");
            sb.AppendLine("## Links");
            sb.AppendLine("- [Support server](https://discord.gg/PEysxvSE3x)");
            sb.AppendLine("- [GitHub Repository](https://github.com/floyzi/YetAnotherFGBot)");
            sb.AppendLine($"- [Built from {YetAnotherFGBotBuildDetails.CommitHash[..8]}](https://github.com/floyzi/YetAnotherFGBot/commit/{YetAnotherFGBotBuildDetails.CommitHash})");
            sb.AppendLine();
            sb.AppendLine("-# Worried about your account? This bot is open sourced, you can verify that your account token isn't saved");
            embed.WithDescription(sb.ToString());

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }

    }
}
