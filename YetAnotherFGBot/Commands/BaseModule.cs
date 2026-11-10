using Discord.Interactions;
using YetAnotherFGBot.ModuleExtension;

namespace YetAnotherFGBot.Commands
{
    public class BaseModule : FGGatewayModule
    {
        [SlashCommand("crown-count", "See your current Fall Guys crown count")]
        public async Task Ping()
        {
            await DeferAsync(true);
            await RequestEOSLogin();
        }
    }
}
