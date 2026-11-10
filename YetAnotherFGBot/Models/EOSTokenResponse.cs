using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace YetAnotherFGBot.Models
{
    internal class EOSTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_at")]
        public DateTime ExpiresAt { get; set; }

        public bool IsValid() => !string.IsNullOrEmpty(AccessToken);
    }
}
