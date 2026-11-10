using System.Text.Json.Serialization;

namespace YetAnotherFGBot.Models
{
    internal class EOSDeviceAuth
    {
        [JsonPropertyName("user_code")]
        public string? UserCode { get; set; }

        [JsonPropertyName("device_code")]
        public string? DeviceCode { get; set; }

        [JsonPropertyName("verification_uri")]
        public string? VerificationURL { get; set; }

        [JsonPropertyName("verification_uri_complete")]
        public string? CompleteVerificationURL { get; set; }

        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }

        [JsonPropertyName("expires_in")]
        public float ExpiresIn { get; set; }

        [JsonPropertyName("interval")]
        public float Interval { get; set; }

        [JsonPropertyName("client_id")]
        public string? ClientID { get; set; }

        public bool IsValid() => !string.IsNullOrEmpty(DeviceCode) && !string.IsNullOrEmpty(VerificationURL);
    }
}
