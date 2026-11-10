using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Text.Json;
using YetAnotherFGBot.Models;

namespace YetAnotherFGBot
{
    public class Program
    {
        static Config? _config;
        static Bot? _bot;
        public static async Task Main()
        {
            _config = JsonSerializer.Deserialize<Config>(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "config.json"))) ?? throw new InvalidOperationException();
            _bot = new(_config.BotToken);

            await Task.Delay(-1);
        }
    }
}
