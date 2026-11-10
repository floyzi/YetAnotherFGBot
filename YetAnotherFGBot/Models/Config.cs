using System.Text.Json.Serialization;

namespace YetAnotherFGBot.Models
{
    internal class Config
    {
        [JsonPropertyName("token")]
        public string? BotToken { get; set; }
    }
}
