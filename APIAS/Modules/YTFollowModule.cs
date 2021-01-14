using Discord.Commands;

using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using APIAS.Db;
using APIAS.Data;

namespace APIAS
{
    class YTFollowModule : ModuleBase
    {
        private Regex _thumbnailRegex = new Regex("media:thumbnail url=\"([^\"]+)");
        private Regex _descriptionRegex = new Regex("media:description>([^<]+)");

        private const string _rssBaseAddress = "https://www.youtube.com/feeds/videos.xml?channel_id=";


        [Command("Setup YT Follow")]
        public async Task SetupYTFollow([Remainder]string ChannelLink)
        {
            if (Globals.InConfigFollows.Any(follow => follow.ConfigUserID() == Context.Message.Author.Id)) {
                await ReplyAsync("You can only configure one follow at the time");
                return;
            }
            YTFollow Follower = new YTFollow(Context);
        }

        [Command("Cancel setup")]
        public async Task CancelConfiguration()
        {
            if (Globals.InConfigFollows.Where(x => x.ConfigUserID() == Context.User.Id).FirstOrDefault() is YTFollow Follow)
                await Follow.CancelConfiguration();
            else
                await ReplyAsync("You don't configure any follow at the moment");
        }

#if DEBUG
        [Command("Fetch")]
        public async Task PullExistant()
        {
            await Globals.Db.GetGuildAysnc(Context.Guild.Id.ToString());
        }
#endif
    }
}
