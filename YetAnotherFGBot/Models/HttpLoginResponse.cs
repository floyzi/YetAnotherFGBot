using System.Text.Json.Serialization;

namespace YetAnotherFGBot.Models
{
    internal class HttpLoginResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("expiry")]
        public DateTime? Expiry { get; set; }

        [JsonPropertyName("accountId")]
        public string? AccountId { get; set; }

        [JsonPropertyName("identities")]
        public Dictionary<string, string>? Identities { get; set; }

        [JsonPropertyName("roles")]
        public string[]? Roles { get; set; }

        [JsonPropertyName("contentUrl")]
        public string? ContentUrl { get; set; }

        [JsonPropertyName("contentVersion")]
        public string? ContentVersion { get; set; }

        [JsonPropertyName("contentEtag")]
        public string? ContentETag { get; set; }

        [JsonPropertyName("additionalProperties")]
        public Dictionary<string, string>? AdditionalProperties { get; set; }

        [JsonPropertyName("tokenType")]
        public string? TokenType { get; set; }

        [JsonPropertyName("clientFeatures")]
        public Dictionary<string, bool>? ClientFeatures { get; set; }

        public bool IsValid() => !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(ContentUrl);
    }
}
