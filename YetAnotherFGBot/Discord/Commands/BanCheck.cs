using Catapult.Protocol;
using Catapult.Runtime.Protocol;
using Catapult.Services.Gateway.Protocol.Client;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using FallGuys.Player.Protocol.Client.Profile;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using YetAnotherFGBot.Discord.ModuleExtension;

namespace YetAnotherFGBot.Discord.Commands
{
    public class BanCheck : FGGatewayModule
    {
        const string EOS_V1_TOKEN = "https://api.epicgames.dev/auth/v1/oauth/token";
        const string EOS_ACTIVE_SANCTIONS = "https://api.epicgames.dev/sanctions/v1/productUser/{0}/active";

        RestFollowupMessage? _pullMsg;

        [SlashCommand("ban-check", "See active restrictions on your Fall Guys account")]
        public async Task Run()
        {
            await DeferAsync(true);
            if (!await RequestFGLogin()) return;

            var embed = new EmbedBuilder();
            embed.WithColor(Color.Orange);
            embed.WithTitle("BAN CHECK");
            embed.WithDescription($"Logged in as ``{EOSName}``, checking ban status...");

            _pullMsg = await Context.Interaction.FollowupAsync(embed: embed.Build());

            await CreateGatewayConnection();
        }

        internal override async Task Receive(Message message)
        {
            if (EOSTokenResponse == null || !EOSTokenResponse.IsValid()) return;

            var embed = new EmbedBuilder();
            bool isBanned = false;
            var sb = new StringBuilder();

            switch (message)
            {
                case Result<AuthenticationResponse> auth:
                    await GatewayConnection!.SendAsync(RuntimeProtocol.CreateMessage(new Message<GetProfileQuery>()
                    {
                        Body = new()
                    }), WebSocketMessageType.Binary, true, CancellationToken.None);
                    break;
                case Result<ProfileDto> profile:
                    try
                    {
                        if (profile.Body == null) throw new InvalidDataException();

                        embed.WithTitle("BAN CHECK RESULT");

                        isBanned = profile.Body.IsBanned;

                        sb.Append($"Account ``{profile.Body.ExternalAccountName}`` is... ");

                        if (!profile.Body.IsBanned)
                            sb.Append("**NOT** banned from matchmaking!");
                        else
                            sb.Append("**BANNED** from matchmaking!");

                        var client = new HttpClient();

                        if (!EOSTokenResponse.IsExpired())
                        {
                            FormUrlEncodedContent payload = new(
                            [
                                new("grant_type", "external_auth"),
                                new("external_auth_type", "epicgames_access_token"),
                                new("external_auth_token", EOSToken),
                                new("deployment_id", Version!.Deployment),
                                new("nonce", $"{Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))}"),
                            ]);

                            var eosUserTokenReq = new HttpRequestMessage(HttpMethod.Post, EOS_V1_TOKEN)
                            {
                                Content = payload,
                                Headers = {
                                        { $"Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Version!.ClientId}:{Version!.ClientSecret}"))}" }
                                }
                            };

                            var eosUserTokenResp = await client.SendAsync(eosUserTokenReq);
                            eosUserTokenResp.EnsureSuccessStatusCode();

                            var eosUserTokenJson = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(await eosUserTokenResp.Content.ReadAsStringAsync());
                            var eosToken = eosUserTokenJson.GetProperty("access_token").GetString();
                            var productId = eosUserTokenJson.GetProperty("product_user_id").GetString();

                            var banCheckReq = new HttpRequestMessage(HttpMethod.Get, string.Format(EOS_ACTIVE_SANCTIONS, productId))
                            {
                                Headers = {
                                    { $"Authorization", $"Bearer {eosToken}" }
                                }
                            };

                            var banSanctionResp = await client.SendAsync(banCheckReq);
                            banSanctionResp.EnsureSuccessStatusCode();
                            var bans = JsonSerializer.Deserialize<JsonElement>(await banSanctionResp.Content.ReadAsStringAsync()).GetProperty("elements").EnumerateArray();

                            bool hasModalBan = bans.Any();
                            if (hasModalBan)
                            {
                                sb.AppendLine();
                                sb.AppendLine();
                                sb.AppendLine($"{(isBanned ? "And" : "But")} has the following game access sanctions:");
                                foreach (var ban in bans)
                                {
                                    var banJson = JsonSerializer.Deserialize<JsonElement>(ban);
                                    var banExp = ban.GetProperty("expirationTimestamp");
                                    sb.AppendLine($"- {ban.GetProperty("action")}: since {TimestampTag.FormatFromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(ban.GetProperty("timestamp").GetInt64()), TimestampTagStyles.LongDateTime)} until {(banExp.ValueKind != JsonValueKind.Number ? "∞" : $"{TimestampTag.FormatFromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(banExp.GetInt64()), TimestampTagStyles.LongDateTime)}")}");
                                }
                            }
                            else
                            {
                                sb.Append(" And has no game access sanctions!");
                            }

                            embed.WithColor(!isBanned ? (hasModalBan ? Color.Orange : Color.Green) : Color.Red);
                        }
                        else
                        {
                            sb.Append($" And i couldn't check if it has game access sanctions, you need to re-login to resolve this issue (your token expired {TimestampTag.FromDateTime(EOSTokenResponse.ExpiresAt, TimestampTagStyles.Relative)})");
                            embed.WithColor(isBanned ? Color.Red : Color.Green);
                        }

                        embed.WithDescription(sb.ToString());
                    }
                    catch (Exception e)
                    {
                        embed.WithDescription($"Failed to run ban check...\n```{e}```");
                    }

                    await _pullMsg!.ModifyAsync(props =>
                    {
                        props.Embed = embed.Build();
                    });

                    await GatewayConnection!.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    break;
            }
        }
    }
}

