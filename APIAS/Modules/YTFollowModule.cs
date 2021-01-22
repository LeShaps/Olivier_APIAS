using Discord.Commands;

using System.Linq;
using System.Collections.Generic;
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
                await Follow.CancelConfigurationAsync();
            else
                await ReplyAsync("You don't configure any follow at the moment");
        }

        [Command("List YT Follows"), Alias("YT Follow list")]
        public async Task GetFollows()
        {
            List<AFollow> YTFollows = Globals.ActiveFollows.Where(follow => follow.GuildID == Context.Guild.Id.ToString() &&
                                                                    follow.Type == FollowType.Youtube).ToList();

            if (YTFollows.Count == 0) {
                await ReplyAsync("You don't have any youtube subscription on this server");
                return;
            }
            await ReplyAsync("Here's a list of your current Youtube subscriptions on this server", false, FollowListBuilder(YTFollows));
        }

        [Command("Stop following")]
        public async Task StopFollow([Remainder]string ChannelName)
        {
            List<AFollow> YTFollows = Globals.ActiveFollows.Where(follow => follow.GuildID == Context.Guild.Id.ToString() &&
                                                                    follow.Type == FollowType.Youtube).ToList();

            foreach (AFollow follow in YTFollows)
            {
                if (((YTFollow)follow).ChannelName == ChannelName) {
                    await follow.RemoveSubscriptionAsync();
                    await ReplyAsync($"You've successfully unfollowed {ChannelName}!");
                    return;
                }
            }

            await ReplyAsync("You're not following any channel with this name");
        }

#if DEBUG
        [Command("Fetch")]
        public async Task PullExistant()
        {
            Globals.Db.GetGuild(Context.Guild.Id.ToString());
        }

        [Command("Deb")]
        public async Task DebugWithAll()
        {
            var c = Globals.ActiveFollows;
            return;
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
