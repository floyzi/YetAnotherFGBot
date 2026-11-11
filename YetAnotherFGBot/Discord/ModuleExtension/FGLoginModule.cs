using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using YetAnotherFGBot.Models;

namespace YetAnotherFGBot.Discord.ModuleExtension
{
    public class FGLoginModule : InteractionModuleBase<SocketInteractionContext>
    {
        class HttpResponse<T>
        {
            public T? ExpectedResponse;
            public JsonElement Json;
            public HttpStatusCode Code;
            public bool ValidJson => Json.ValueKind != JsonValueKind.Undefined && Json.ValueKind != JsonValueKind.Null;
            public bool Ok => ExpectedResponse != null && ExpectedResponse is not null;

            public async Task Set(HttpResponseMessage resp)
            {
                var responseBody = await resp.Content.ReadAsStringAsync();

                try
                {
                    Code = resp.StatusCode;
                    Json = JsonSerializer.Deserialize<JsonElement>(responseBody);
                    ExpectedResponse = Json.Deserialize<T>();
                }
                catch
                {
                }
            }
        }

        const string VERSION_INFO = "https://floyzi-gitlab-io.vercel.app/fallguys/version.json";
        const string DEVICE_AUTH = "https://api.epicgames.dev/epic/oauth/v2/deviceAuthorization";
        const string EOS_V2_TOKEN = "https://api.epicgames.dev/epic/oauth/v2/token";
        const string FG_LOGIN = "https://login.fallguys.oncatapult.com/api/v1/login";

        protected FGVersionInfo? Version;
        protected EOSTokenResponse? EOSTokenResponse;
        protected string? EOSToken => EOSTokenResponse?.AccessToken;
        protected HttpLoginResponse? FGLoginResponse;
        protected string? FGToken => FGLoginResponse?.Token;
        protected string? EOSName;

        protected async Task<bool> RequestFGLogin()
        {
            var embed = new EmbedBuilder();
            var comp = new ComponentBuilder();

            using var client = new HttpClient();
            var verReq = new HttpRequestMessage(HttpMethod.Get, VERSION_INFO);

            var verResp = await client.SendAsync(verReq);
            var verRespBody = await verResp.Content.ReadAsStringAsync();
            Version = JsonSerializer.Deserialize<FGVersionInfo>(verRespBody);

            embed.WithTitle("FALL GUYS LOGIN");

            if (Version == null)
            {
                embed.WithColor(Color.Red);
                embed.WithDescription($"Coldn't obtain version info, try again later.");
                await Context.Interaction.FollowupAsync(ephemeral: true, embed: embed.Build());
                embed.WithThumbnailUrl(Emotes.Cross.Url);
                return false;
            }

            var request = await CreateEOSLoginRequest();

            if (!request.Ok || !request.ExpectedResponse!.IsValid())
            {
                embed.WithColor(Color.Red);
                embed.WithDescription($"Unable to create login request (http {request.Code}), try again later.{(request.ValidJson ? $"\n```\njson{request.Json}```" : "")}");
                embed.WithThumbnailUrl(Emotes.Cross.Url);
                await Context.Interaction.FollowupAsync(ephemeral: true, embed: embed.Build());
                return false;
            }

            embed.WithColor(Color.Orange);
            embed.WithDescription($"This command requires you to log in, click on the button below (or [here]({request.ExpectedResponse.CompleteVerificationURL})) to continue.\nOnce you log in this message will be updated.\n\n-# Bot will only receive access to your Fall Guys account, not your Epic Account.\n-# None of the login information is saved externally.\n-# See about to learn more on how this works");
            embed.WithFooter($"Link will expire in {TimeSpan.FromSeconds(request.ExpectedResponse.ExpiresIn).Minutes} minutes");
            embed.WithThumbnailUrl(Emotes.InvisibeansRun.Url);
            embed.WithCurrentTimestamp();

            var builder = comp.WithButton("Login", style: ButtonStyle.Link, url: request.ExpectedResponse.CompleteVerificationURL);
            await Context.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true, components: builder.Build());

            var tokenResponse = await WaitForLoginApproval(request.ExpectedResponse);

            if (!tokenResponse.Ok || !tokenResponse.ExpectedResponse!.IsValid())
            {
                embed.WithColor(Color.Red);
                embed.WithDescription($"Unable to complete login attempt (http {tokenResponse.Code}), try again later.{(request.ValidJson ? $"\n```json\n{request.Json}```" : "")}");
                embed.WithThumbnailUrl(Emotes.Cross.Url);
                await ModifyOriginalResponseAsync(props =>
                {
                    props.Embed = embed.Build();
                    props.Components = null;
                });
                return false;
            }

            EOSTokenResponse = tokenResponse.ExpectedResponse;

            embed.Footer = null;
            embed.WithDescription($"EOS login completed, logging in Fall Guys...");
            embed.WithThumbnailUrl(Emotes.Tick.Url);

            await ModifyOriginalResponseAsync(props =>
            {
                props.Embed = embed.Build();
                props.Components = null;
            });

            var fgToken = await FGLogin();
            if (!fgToken.Ok || !fgToken.ExpectedResponse!.IsValid())
            {
                embed.WithColor(Color.Red);
                if (!fgToken.ValidJson)
                    embed.WithDescription($"Unable to complete Fall Guys login attempt (http {tokenResponse.Code}), try again later.");
                else
                {
                    if (request.Json.TryGetProperty("errorCode", out var errCode) && errCode.TryGetInt32(out var code))
                    {
                        switch (code)
                        {
                            case 201000017:
                                embed.WithDescription($"Your Fall Guys account was permanently banned and it can't be used for login.");
                                break;
                            default:
                                embed.WithDescription($"Unable to complete Fall Guys login attempt (http {tokenResponse.Code}), Fall Guys replied with unknown error code ``{code}``");
                                break;
                        }
                    }
                    else
                        embed.WithDescription($"Unable to complete Fall Guys login attempt (http {tokenResponse.Code}), try again later.\n```\njson{request.Json}```");

                }

                embed.WithThumbnailUrl(Emotes.Cross.Url);
                await ModifyOriginalResponseAsync(props =>
                {
                    props.Embed = embed.Build();
                    props.Components = null;
                });
                return false;
            }

            FGLoginResponse = fgToken.ExpectedResponse;
            if (fgToken.ExpectedResponse.AdditionalProperties == null) return false;

            EOSName = fgToken.ExpectedResponse.AdditionalProperties["EosDisplayName"];

            await Context.Interaction.DeleteOriginalResponseAsync();
            return !string.IsNullOrEmpty(EOSToken) && !string.IsNullOrEmpty(FGToken);
        }

        async Task<HttpResponse<EOSDeviceAuth>> CreateEOSLoginRequest()
        {
            var res = new HttpResponse<EOSDeviceAuth>();
            var client = new HttpClient();

            var response = await client.PostAsync(DEVICE_AUTH, new FormUrlEncodedContent(
            [
                new("prompt", "login"),
                new("client_id", Version!.ClientId),
                new("client_secret", Version.ClientSecret)
            ]));

            await res.Set(response);

            return res;
        }

        async Task<HttpResponse<EOSTokenResponse>> WaitForLoginApproval(EOSDeviceAuth loginRequest)
        {
            var res = new HttpResponse<EOSTokenResponse>();
            var sw = new Stopwatch();
            sw.Start();

            var clientAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Version!.ClientId}:{Version!.ClientSecret}"));
            using var client = new HttpClient();

            while (sw.Elapsed.Seconds < loginRequest.ExpiresIn)
            {
                FormUrlEncodedContent payload = new(
                [
                    new("grant_type", "device_code"),
                    new("deployment_id", Version!.Deployment),
                    new("scope", "basic_profile openid"),
                    new("device_code", loginRequest.DeviceCode),
                ]);

                var request = new HttpRequestMessage(HttpMethod.Post, EOS_V2_TOKEN)
                {
                    Content = payload,
                    Headers = {
                        { $"Authorization", $"Basic {clientAuth}" }
                    }
                };
                 
                var response = await client.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    await Task.Delay(TimeSpan.FromSeconds(loginRequest.Interval));
                    continue;
                }

                await res.Set(response);

                return res;
            }

            sw.Stop();

            return new();
        }

        async Task<HttpResponse<HttpLoginResponse>> FGLogin()
        {
            if (string.IsNullOrEmpty(EOSToken)) throw new InvalidOperationException();

            var res = new HttpResponse<HttpLoginResponse>();
            using var client = new HttpClient();

            var loginReq = new HttpRequestMessage(HttpMethod.Post, FG_LOGIN)
            {
                Content = new StringContent(LoginModel.Create(EOSToken, Version!), Encoding.UTF8, "application/json"),
            };
            loginReq.Headers.Add("X-Unity-Version", Version!.UnityVersion);
            loginReq.Headers.Add("User-Agent", $"UnityPlayer/{Version.UnityVersion} (UnityWebRequest/1.0, libcurl/7.84.0-DEV)");

            var authResp = await client.SendAsync(loginReq);

            await res.Set(authResp);

            return res;
        }
    }
}
