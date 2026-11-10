using Catapult.Protocol;
using Catapult.Runtime.Protocol;
using Catapult.Services.Gateway.Protocol.Client;
using System.Net.WebSockets;
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

                    await Receive(RuntimeProtocol.ReadMessage(ms));
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

        internal abstract Task Receive(Message message);
    }
}
