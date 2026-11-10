using Catapult.Protocol;
using Catapult.Runtime.Protocol;
using Catapult.Services.Gateway.Protocol.Client;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.Rest;
using FallGuys.Player.Protocol.Client.PlayerLevel;
using FallGuys.Player.Protocol.Client.Profile;
using System.Net.WebSockets;
using YetAnotherFGBot.ModuleExtension;

namespace YetAnotherFGBot.Commands
{
    public class CrownCount : FGGatewayModule
    {
        RestFollowupMessage? _pullMsg;

        [SlashCommand("crown-count", "See your current Fall Guys crown count")]
        public async Task PullCrowns()
        {
            await DeferAsync(true);
            if (!await RequestFGLogin()) return;

            var embed = new EmbedBuilder();
            embed.WithColor(Color.Orange);
            embed.WithTitle("CROWN COUNT");
            embed.WithDescription($"Logged in as ``{EOSName}``, pulling crown count data...");

            _pullMsg = await Context.Interaction.FollowupAsync(embed: embed.Build());
  
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
                    var emoji = new Emote(1569735482596917349, "UI_currency_crown_outline");

                    var embed = new EmbedBuilder();
                    embed.WithColor(Color.Green);
                    embed.WithTitle("CROWN COUNT");
                    embed.WithThumbnailUrl(emoji.Url);
                    embed.WithDescription($"Account ``{EOSName}`` has **{level.Body?.EarnedCrowns:N0}** crowns");

                    await _pullMsg!.ModifyAsync(props =>
                    {
                        props.Embed = embed.Build();
                    });
                    break;
            }
        }
    }
}
