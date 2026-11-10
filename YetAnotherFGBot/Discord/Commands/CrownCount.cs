using Catapult.Protocol;
using Catapult.Runtime.Protocol;
using Catapult.Services.Gateway.Protocol.Client;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using FallGuys.Player.Protocol.Client.PlayerLevel;
using System.Net.WebSockets;
using YetAnotherFGBot.Discord.ModuleExtension;

namespace YetAnotherFGBot.Discord.Commands
{
    public class CrownCount : FGGatewayModule
    {
        RestFollowupMessage? _pullMsg;

        [SlashCommand("crown-count", "See your current Fall Guys crown count")]
        public async Task Run()
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
                    var embed = new EmbedBuilder();
                    embed.WithColor(Color.Green);
                    embed.WithTitle("CROWN COUNT");
                    embed.WithThumbnailUrl(Emotes.Crown.Url);
                    embed.WithDescription($"Account ``{EOSName}`` has **{level.Body?.EarnedCrowns:N0}** crowns");

                    await _pullMsg!.ModifyAsync(props =>
                    {
                        props.Embed = embed.Build();
                    });

                    await GatewayConnection!.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    break;
            }
        }
    }
}
