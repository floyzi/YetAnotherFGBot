using System.Text.Json.Serialization;

namespace YetAnotherFGBot.Models
{
    internal class Config
    {
        [JsonPropertyName("token")]
        public string? BotToken { get; set; }

        [JsonPropertyName("fg_ver")]
        public string? FGVersion { get; set; }

        [JsonPropertyName("fg_signature")]
        public string? FGSignature { get; set; }

        [JsonPropertyName("fg_client_id")]
        public string? ClientId { get; set; }

        [JsonPropertyName("fg_client_secret")]
        public string? ClientSecret { get; set; }

        [JsonPropertyName("fg_unity_ver")]
        public string? UnityVersion { get; set; }

    }
}
