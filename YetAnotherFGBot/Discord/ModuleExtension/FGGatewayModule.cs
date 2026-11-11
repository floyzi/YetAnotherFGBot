using Catapult.Protocol;
using Catapult.Protocol.Internal;
using Catapult.Runtime.Protocol;
using Catapult.Services.Gateway.Protocol.Client;
using Discord;
using System.Net.WebSockets;
using System.Text.Json;
using YetAnotherFGBot.Models;

namespace YetAnotherFGBot.Discord.ModuleExtension
{
    public abstract class FGGatewayModule : FGLoginModule
    {
        protected ClientWebSocket? GatewayConnection;

        protected async Task CreateGatewayConnection()
        {
            var gatewayConn = new Uri("wss://gateway.fallguys.oncatapult.com:443/ws");
            GatewayConnection = new ClientWebSocket();

            await GatewayConnection.ConnectAsync(gatewayConn, CancellationToken.None);

            async Task listen()
            {
                while (GatewayConnection.State == WebSocketState.Open)
                {
                    using var ms = new MemoryStream();
                    var buffer = new byte[4096];

                    WebSocketReceiveResult res;
                    do
                    {
                        res = await GatewayConnection.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (res.MessageType == WebSocketMessageType.Close)
                            throw new WebSocketException("Socket closed while reading message.");

                        ms.Write(buffer, 0, res.Count);

                    } while (!res.EndOfMessage);

                    await InternalReceive(RuntimeProtocol.ReadMessage(ms));
                }
            }

            _ = listen();

            await GatewayConnection.SendAsync(RuntimeProtocol.CreateMessage(new Message<AuthenticationRequest>()
            {
                Body = new()
                {
                    Token = FGToken,
                    ContentEtag = "",
                }
            }), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        async Task InternalReceive(Message? message)
        {
            if (message == null) return;

            if (message is ErrorMessage error)
            {
                var embed = new EmbedBuilder();
                embed.WithColor(Color.Red);
                embed.WithThumbnailUrl(Emotes.Cross.Url);
                embed.WithTitle("ERROR");
                embed.WithDescription($"Fall Guys returned an unhandled error\n```json\n{JsonSerializer.Serialize(error)}```");
                embed.WithFooter("If you sure that this error is caused by the bot report it on the support server");

                await Context.Interaction.FollowupAsync(embed: embed.Build());
                return;
            }

            await Receive(message);
        }

        internal abstract Task Receive(Message message);
    }
}
