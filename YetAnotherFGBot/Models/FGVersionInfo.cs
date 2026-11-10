using System.Text.Json.Serialization;

namespace YetAnotherFGBot.Models
{
    public class FGVersionInfo
    {
        [JsonPropertyName("clientVersion")]
        public string? Version { get; set; }

        [JsonPropertyName("clientVersionSignature")]
        public string? Signature { get; set; }

        [JsonPropertyName("playerVersion")]
        public string? UnityVersion { get; set; }

        [JsonPropertyName("deploymentId")]
        public string? Deployment { get; set; }

        [JsonPropertyName("clientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("clientSecret")]
        public string? ClientSecret { get; set; }
    }
}
