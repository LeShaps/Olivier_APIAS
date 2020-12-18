using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace APIAS
{
    class CommunicationModule : ModuleBase
    {
        [Command("Who are you?")]
        public async Task Presentation()
        {
            await ReplyAsync("Greetings, dear user, I'm Olivier APIAS, and I'm here to watch over information's flux, you'll know more soon, but I'm afraid I couldn't repond to all your questions");
        }
    }
}
