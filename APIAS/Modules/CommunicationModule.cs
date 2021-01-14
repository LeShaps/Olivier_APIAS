using Discord;
using Discord.Commands;

using System.Threading.Tasks;

namespace APIAS
{
    class CommunicationModule : ModuleBase
    {
        [Command("Who are you?")]
        public async Task Presentation()
        {
            await ReplyAsync("Greetings, dear user, I'm Olivier APIAS, and I'm here to watch over information's flux, you'll know more soon");
        }

        [Command("Help")]
        public async Task SendHelp()
        {
            await ReplyAsync("", false, MakeHelp());
        }

        private Embed MakeHelp()
        {
            string Description = "**Follows**\nSetup YT Follow - Setup a youtube follow on this server\n" +
                "Cancel Setup - End a current configuration that you make\n" +
                "List YT Follows - List all the youtube channels you're currently following\n" +
                "Stop following - Stop following a Youtube channel";

            EmbedBuilder builder = new EmbedBuilder
            {
                Title = "Help",
                Description = Description,
                Color = Color.DarkBlue
            };

            return builder.Build();
        }
    }
}
