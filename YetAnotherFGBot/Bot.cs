using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;

namespace YetAnotherFGBot
{
    internal class Bot
    {
        readonly DiscordSocketClient? _client;
        readonly InteractionService? _interactions;

        internal Bot(string? token)
        {
            if (string.IsNullOrEmpty(token)) return;

            _client = new DiscordSocketClient();
            _interactions = new(_client.Rest);

            _client.Log += Log;
            _client.InteractionCreated += HandleInteraction;

            _ = LoginWithToken(token);
        }

        async Task LoginWithToken(string token)
        {
            if (_client == null) throw new InvalidOperationException();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.Ready += async () =>
            {
                if (_interactions == null) return;

                await _interactions.AddModulesAsync(typeof(Bot).Assembly, null);
                await _interactions.RegisterCommandsGloballyAsync();
            };

            await Task.Delay(-1);
        }

        async Task HandleInteraction(SocketInteraction interaction)
        {
            if (_interactions == null) return;

            await _interactions.ExecuteCommandAsync(new SocketInteractionContext(_client, interaction), null);
        }

        async Task Log(LogMessage message)
        {
            Console.WriteLine(message);
        }
    }
}
