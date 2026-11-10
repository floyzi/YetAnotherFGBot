using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace YetAnotherFGBot.Models
{
    internal class LoginModel
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, object>? Properties { get; set; }

        [JsonPropertyName("userParameters")]
        public Dictionary<string, object>? UserParameters { get; set; }

        [JsonPropertyName("clientVersion")]
        public string? ClientVersion { get; set; }

        [JsonPropertyName("clientVersionSignature")]
        public string? ClientVersionSignature { get; set; }

        [JsonPropertyName("platform")]
        public string? Platform { get; set; }

        [JsonPropertyName("contentBranch")]
        public string? ContentBranch { get; set; }

        [JsonPropertyName("dto")]
        public object? Dto { get; set; }

        internal static string Create(string token, FGVersionInfo verInfo)
        {
            if (verInfo == null) return null!;

            return JsonConvert.SerializeObject(new LoginModel()
            {
                Type = "EosSignIn",
                Token = token,
                Properties = [],
                UserParameters = new Dictionary<string, object>()
                    {
                        { "lang", "EN" },
                        { "loc", "en-US" },
                    },
                ClientVersion = verInfo.Version,
                ClientVersionSignature = verInfo.Signature,
                ContentBranch = "",
                Platform = "pc_egs",
                Dto = { }
            });
        }
    }
}
