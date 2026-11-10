using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Text.Json;
using YetAnotherFGBot.Models;

namespace YetAnotherFGBot
{
    public class Program
    {
        static DiscordSocketClient? _client;
        static Config? _config;
        public static async Task Main()
        {
            _config = JsonSerializer.Deserialize<Config>(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "config.json"))) ?? throw new InvalidOperationException();

            _client = new DiscordSocketClient();
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, _config.BotToken);
            await _client.StartAsync();

            _client.SlashCommandExecuted += HandleSlashCommand;
            _client.Ready += async () =>
            {
                try
                {
                    var crowsCommand = new SlashCommandBuilder().WithName("crown-count").WithDescription("Crown Count").Build();
                    await _client.CreateGlobalApplicationCommandAsync(crowsCommand);
                    Console.WriteLine($"created");
                }
                catch (HttpException ex)
                {
                    Console.WriteLine($"HTTP error: {ex.HttpCode}");
                    Console.WriteLine(ex.Message);
                }

                return;
            };

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        static async Task HandleSlashCommand(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "crown-count":
                    await command.RespondAsync("Pong!");
                    break;
                case "hello":
                    break;
            }
        }

        static async Task Log(LogMessage message)
        {
            Console.WriteLine(message);
        }
    }
}
