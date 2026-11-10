using Catapult.Protocol;
using Catapult.Runtime.Protocol;
using Catapult.Services.Gateway.Protocol.Client;
using Discord;
using Discord.Interactions;
using Discord.Net;
using FallGuys.Player.Protocol.Client.PlayerLevel;
using FallGuys.Player.Protocol.Client.Profile;
using System.Net.WebSockets;
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

            await CreateGatewayConnection();
        }

        internal override async Task Receive(Message message)
        {
            Console.WriteLine(message.GetTypeName());

            switch (message)
            {
                case Result<AuthenticationResponse>:
                    await GatewayConnection!.SendAsync(RuntimeProtocol.CreateMessage(new Message<GetPlayerLevelQuery>()
                    {
                        Body = new()
                    }), WebSocketMessageType.Binary, true, CancellationToken.None);
                    break;
                case Result<PlayerLevelDto> level:
                    var embed = new EmbedBuilder();
                    embed.WithColor(Color.Green);
                    embed.WithTitle("CROWN COUNT");
                    embed.WithDescription($"``{EOSName}`` has ``{level.Body?.EarnedCrowns:F0}`` crowns");
                    await Context.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true);
                    break;
            }
        }
    }
}
