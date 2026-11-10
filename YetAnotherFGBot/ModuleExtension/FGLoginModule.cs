using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using YetAnotherFGBot.Models;

namespace YetAnotherFGBot.ModuleExtension
{
    public class FGLoginModule : InteractionModuleBase<SocketInteractionContext>
    {
        const string VERSION_INFO = "https://floyzi-gitlab-io.vercel.app/fallguys/version.json";
        const string DEVICE_AUTH = "https://api.epicgames.dev/epic/oauth/v2/deviceAuthorization";
        const string EOS_TOKEN = "https://api.epicgames.dev/epic/oauth/v2/token";
        const string FG_LOGIN = "https://login.fallguys.oncatapult.com/api/v1/login";

        protected FGVersionInfo? Version;
        protected string? EOSToken;
        protected string? FGToken;
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
                return false;
            }

            var request = await CreateEOSLoginRequest();

            if (request.Item1 == null || !request.Item1.IsValid())
            {
                embed.WithColor(Color.Red);
                embed.WithDescription($"Unable to create login request (http {request.Item2}), try again later.");
                await Context.Interaction.FollowupAsync(ephemeral: true, embed: embed.Build());
                return false;
            }

            embed.WithColor(Color.Orange);
            embed.WithDescription($"To log in to your Fall Guys account click on the button below (or [here]({request.Item1.CompleteVerificationURL})).\nOnce you log in this message will be updated.\n\n-# Bot will only receive access to your Fall Guys account, not your Epic Account.\n-# None of the login information is saved externally.");
            embed.WithFooter($"Link will expire in {TimeSpan.FromSeconds(request.Item1.ExpiresIn).Minutes} minutes");
            embed.WithCurrentTimestamp();

            var builder = comp.WithButton("Login", style: ButtonStyle.Link, url: request.Item1.CompleteVerificationURL);
            await Context.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true, components: builder.Build());

            var tokenResponse = await WaitForLoginApproval(request.Item1);

            if (tokenResponse.Item1 == null || !tokenResponse.Item1.IsValid())
            {
                embed.WithColor(Color.Red);
                embed.WithDescription($"Unable to complete login attempt (http {tokenResponse.Item2}), try again later.");
                await ModifyOriginalResponseAsync(props =>
                {
                    props.Embed = embed.Build();
                    props.Components = null;
                });
                return false;
            }

            EOSToken = tokenResponse.Item1.AccessToken;

            embed.Footer = null;
            embed.WithDescription($"Logging in Fall Guys...");
            await ModifyOriginalResponseAsync(props =>
            {
                props.Embed = embed.Build();
                props.Components = null;
            });

            var fgToken = await FGLogin();
            if (fgToken.Item1 == null || !fgToken.Item1.IsValid())
            {
                embed.WithColor(Color.Red);
                embed.WithDescription($"Unable to complete Fall Guys login attempt (http {tokenResponse.Item2}), try again later.");
                await ModifyOriginalResponseAsync(props =>
                {
                    props.Embed = embed.Build();
                    props.Components = null;
                });
                return false;
            }

            FGToken = fgToken.Item1.Token;
            if (fgToken.Item1.AdditionalProperties == null) return false;

            EOSName = fgToken.Item1.AdditionalProperties["EosDisplayName"];

            await Context.Interaction.DeleteOriginalResponseAsync();
            return !string.IsNullOrEmpty(EOSToken) && !string.IsNullOrEmpty(FGToken);
        }

        async Task<(EOSDeviceAuth, HttpStatusCode)> CreateEOSLoginRequest()
        {
            var client = new HttpClient();

            var response = await client.PostAsync(DEVICE_AUTH, new FormUrlEncodedContent(
            [
                new("prompt", "login"),
                new("client_id", Version!.ClientId),
                new("client_secret", Version.ClientSecret)
            ]));

            var responseBody = await response.Content.ReadAsStringAsync();

            EOSDeviceAuth res;

            try
            {
                res = JsonSerializer.Deserialize<EOSDeviceAuth>(responseBody)!;
            }
            catch
            {
                res = null!;
            }

            return new(res, response.StatusCode);
        }

        async Task<(EOSTokenResponse, HttpStatusCode)> WaitForLoginApproval(EOSDeviceAuth loginRequest)
        {
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

                var request = new HttpRequestMessage(HttpMethod.Post, EOS_TOKEN)
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

                var responseBody = await response.Content.ReadAsStringAsync();

                EOSTokenResponse res;

                try
                {
                    res = JsonSerializer.Deserialize<EOSTokenResponse>(responseBody)!;
                }
                catch
                {
                    res = null!;
                }

                return new(res, response.StatusCode);
            }

            sw.Stop();

            return new();
        }

        async Task<(HttpLoginResponse, HttpStatusCode)> FGLogin()
        {
            using var client = new HttpClient();

            var loginReq = new HttpRequestMessage(HttpMethod.Post, FG_LOGIN)
            {
                Content = new StringContent(LoginModel.Create(EOSToken, Version!), Encoding.UTF8, "application/json"),
            };
            loginReq.Headers.Add("X-Unity-Version", Version!.UnityVersion);
            loginReq.Headers.Add("User-Agent", $"UnityPlayer/{Version.UnityVersion} (UnityWebRequest/1.0, libcurl/7.84.0-DEV)");


            var authResp = await client.SendAsync(loginReq);
            var authRespBody = await authResp.Content.ReadAsStringAsync();


            if (authResp.StatusCode != System.Net.HttpStatusCode.OK)
                return new(null, authResp.StatusCode);

            try
            {
                var json = JsonSerializer.Deserialize<HttpLoginResponse>(authRespBody);
                return new(json, authResp.StatusCode);
            }
            catch
            {
                return new(null, authResp.StatusCode);
            }
        }
    }
}
