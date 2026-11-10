using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace YetAnotherFGBot.Models
{
    public class EOSTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_at")]
        public DateTime ExpiresAt { get; set; }

        public bool IsValid() => !string.IsNullOrEmpty(AccessToken);
        public bool IsExpired() => DateTime.UtcNow > ExpiresAt.ToUniversalTime();
    }
}
