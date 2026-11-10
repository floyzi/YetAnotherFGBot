using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace YetAnotherFGBot.Discord
{
    internal static class Emotes
    {
        internal static Emote Crown { get; } = new Emote(1569735482596917349, "UI_currency_crown_outline");
        internal static Emote InvisibeansRun { get; } = new Emote(1569753520976961536, "UI_invisibeans_run");
        internal static Emote Lock { get; } = new Emote(1569753734295068782, "UI_SeasonPass_Lock");
        internal static Emote LockOpen { get; } = new Emote(1569753871943864501, "UI_SeasonPass_LockOpen");
        internal static Emote Tick { get; } = new Emote(1569753970635968522, "Icon_Tick_Large");
    }
}
