using Discord.Commands;

using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using APIAS.Db;
using APIAS.Data;
using APIAS.Abstracts;
using Discord;

namespace APIAS
{
    class YTFollowModule : ModuleBase
    {
        [Command("Setup YT Follow")]
        public async Task SetupYTFollow()
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

        [Command("List YT Follows"), Alias("YT Follow list")]
        public async Task GetFollows()
        {
            List<AFollow> YTFollows = Globals.ActiveFollows.Where(follow => follow.GuildID == Context.Guild.Id.ToString() &&
                                                                    follow.Type == FollowType.Youtube).ToList();

            await ReplyAsync("Here's a list of your current Youtube subscriptions on this server", false, FollowListBuilder(YTFollows));
        }

#if DEBUG
        [Command("Fetch")]
        public async Task PullExistant()
        {
            await Globals.Db.GetGuildAysnc(Context.Guild.Id.ToString());
        }
#endif

        private Embed FollowListBuilder(List<AFollow> follows)
        {
            string FollowList = "";

            foreach (AFollow fl in follows)
            {
                FollowList += $"{((YTFollow)fl).ChannelName}\n";
            }

            EmbedBuilder builder = new EmbedBuilder
            {
                Title = "Youtube subscriptions",
                Color = Color.DarkBlue,
                Description = FollowList
            };

            return builder.Build();
        }
    }
}
