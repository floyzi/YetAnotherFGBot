using Discord;
using Discord.WebSocket;

namespace YetAnotherFGBot
{
    internal class Bot
    {
        static DiscordSocketClient? _client;

        internal Bot(string? token)
        {
            if (string.IsNullOrEmpty(token)) return;

            _client = new DiscordSocketClient();
            _client.Log += Log;

            _ = LoginWithToken(token);
        }

        async Task LoginWithToken(string token)
        {
            if (_client == null) throw new InvalidOperationException();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.SlashCommandExecuted += HandleSlashCommand;
            _client.Ready += async () =>
            {
                var crowsCommand = new SlashCommandBuilder().WithName("crown-count").WithDescription("Crown Count").Build();
                await _client.CreateGlobalApplicationCommandAsync(crowsCommand);
            };

            await Task.Delay(-1);
        }

        async Task HandleSlashCommand(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "crown-count":
                    await command.RespondAsync("Pong!");
                    break;
            }
        }

        async Task Log(LogMessage message)
        {
            Console.WriteLine(message);
        }
    }
}
