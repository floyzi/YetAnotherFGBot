using Catapult.Runtime.Protocol;
using System.Text.Json;
using YetAnotherFGBot.Discord;
using YetAnotherFGBot.Models;

namespace YetAnotherFGBot
{
    public class Program
    {
        internal static DateTime Startup;

        static Config? _config;
        static Bot? _bot;
        
        public static async Task Main()
        {
            Startup = DateTime.UtcNow;

            _config = JsonSerializer.Deserialize<Config>(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "config.json"))) ?? throw new InvalidOperationException();

            RuntimeProtocol.Initialize();

            _bot = new(_config.BotToken);

            await Task.Delay(-1);
        }
    }
}
