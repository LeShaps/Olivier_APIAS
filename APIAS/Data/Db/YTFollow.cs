using Discord;
using Discord.Commands;

using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ServiceModel.Syndication;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using APIAS.Data;
using APIAS.Utilities;
using APIAS.Abstracts;

namespace APIAS.Db
{
    class YTFollow : AFollow
    {
        /* Youtube channel infos */
        public string ChannelName;
        public string ChannelUrl;
        public string ChannelIconUrl;
        public string ChannelDescription;

        /* Youtube updates infos */
        public string LastKnownVideo;
        public DateTime LastPublicationDate;
        public string LastVideoIconUrl;
        public string LastVideoDescription;
        public string LastVideoUrl;

        public string RssAdress;

        /* Initial configuration and update */
        private Regex _thumbnailRegex = new Regex("media:thumbnail url=\"([^\"]+)");
        private Regex _descriptionRegex = new Regex("media:description>([^<]+)");

        private const string _rssBaseAddress = "https://www.youtube.com/feeds/videos.xml?channel_id=";

        // For RethinkDb to rebuild it when done
        public YTFollow() { }

        public YTFollow(ICommandContext Context)
        {
            _configurationGuild = Context.Guild;
            SendInitialMessage(Context.Message);
            GuildID = Context.Guild.Id.ToString();

            _gates = new List<ConfigurationUpdater>
            {
                FindUserChannel,
                SetupFrequency,
                SetupFollowChannel,
                AddPingRoles,
                Finish
            };
            Type = FollowType.Youtube;
        }

        /* Implementation */
        public override async void CheckUpdate(object? obj)
        {
            string RSSToRead = new WebClient().DownloadString(RssAdress);
            using var Reader = XmlReader.Create(new StringReader(RSSToRead));

            SyndicationFeed feed = SyndicationFeed.Load(Reader);
            var post = feed.Items.FirstOrDefault();

            if (!_isConfigFinished)
            {
                SetNewVideoInfos(post, RSSToRead);
                return;
            }
            else if (LastPublicationDate.CompareTo(post.PublishDate.DateTime) < 0)
            {
                SetNewVideoInfos(post, RSSToRead);
                await SendUpdateMessage();
            }
            else
                return;
        }

        public override async Task SendUpdateMessage()
        {
            string Mentions = "";
            foreach (ulong user in _mensionRoles)
            {
                Mentions += $"<@!{user}> ";
            }
            Mentions += $"{ChannelName} has uploaded a new video!";

            EmbedBuilder NewVideoEmbed = new EmbedBuilder
            {
                Color = Color.Red,
                ImageUrl = LastVideoIconUrl,
                Url = LastVideoUrl,
                Description = LastVideoDescription,
                Title = LastKnownVideo
            };

            foreach (IGuildChannel chan in _mensionChannels)
            {
                await ((ITextChannel)chan).SendMessageAsync(Mentions, false, NewVideoEmbed.Build());
            }
        }

        private async void SendInitialMessage(IMessage MessageContext)
        {
            _configUserID = MessageContext.Author.Id;
            _configurationGate = 0;
            _message = await MessageContext.Channel.SendMessageAsync("", false, new EmbedBuilder
            {
                Title = "Setting up now channel",
                Description = "Please enter the channel's adress (name is to come later)",
                Color = new Color(16718362),
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Configuration made by {MessageContext.Author.Username}"
                }
            }.Build());
            Globals.InConfigFollows.Add(this);
        }

        /* Gates */
        private async void FindUserChannel(IMessage MessageContext)
        {
            InitYoutubeInfos(MessageContext.Content.ReplaceAll("", "<", ">"));
            EmbedBuilder UpdateBuilder = _message.Embeds.First().ToEmbedBuilder();

            UpdateBuilder.Description = "Are theses the right informations?";
            UpdateBuilder.Fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    Name = "Name",
                    Value = ChannelName,
                    IsInline = true
                },
                new EmbedFieldBuilder
                {
                    Name = "Description",
                    Value = ChannelDescription,
                    IsInline = false
                }
            };
            UpdateBuilder.ImageUrl = ChannelIconUrl;
            UpdateBuilder.Url = ChannelUrl;

            await _message.ModifyAsync(x => x.Embed = UpdateBuilder.Build());
            await _message.AddReactionsAsync(Utilities.Utilities.MakeEmojiArray("✅", "🚫"));
            _configurationGate++;
        }

        private async void SetupFrequency(IMessage MessageContext)
        {
            await _message.RemoveAllReactionsAsync();
            EmbedBuilder UpdateBuilder = _message.Embeds.First().ToEmbedBuilder();

            UpdateBuilder.Fields.Clear();
            UpdateBuilder.ImageUrl = null;
            UpdateBuilder.Url = null;

            if (_pendingReaction.Emote.Name == "🚫")
            {
                UpdateBuilder.Title = "End configuration";
                UpdateBuilder.Description = "The multiple results feature isn't ready yet, wait for it...";

                await _message.ModifyAsync(x => x.Embed = UpdateBuilder.Build());
                Globals.InConfigFollows.Remove(this);
                return;
            }


            UpdateBuilder.Title = "Setting up now frequency";
            UpdateBuilder.Description = $"At which frequency do you want to check the channel?\n1. Every day\n" +
                $"2. Every 12 hours\n" +
                $"3. Every 6 hours\n" +
                $"4. Every hour\n" +
                $"5. Every 10 minutes\n" +
                $"6. Custom\n\nNote: this doesn't have any consequences for the moment";

            await _message.ModifyAsync(x => x.Embed = UpdateBuilder.Build());
            await _message.AddReactionsAsync(Utilities.Utilities.MakeEmojiArray("1️⃣", "2️⃣", "3️⃣", "4️⃣", "5️⃣", "6️⃣"));
            _configurationGate++;
        }

        private async void SetupFollowChannel(IMessage MessageContext)
        {
            RefreshTime = _pendingReaction.Emote.Name switch
            {
                "1️⃣" => 24 * ((1000 * 60) * 60),
                "2️⃣" => 12 * ((1000 * 60) * 60),
                "3️⃣" => 6 * ((1000 * 60) * 60),
                "4️⃣" => ((1000 * 60) * 60),
                "5️⃣" => 600000,
                _ => 24 * ((1000 * 60) * 60)
            };
            _updateRoutineTimer = new Timer(new TimerCallback(CheckUpdate), null, 0, RefreshTime);
            await _message.RemoveAllReactionsAsync();
            EmbedBuilder UpdateBuilder = _message.Embeds.First().ToEmbedBuilder();

            UpdateBuilder.Title = "Setting up notifications";
            UpdateBuilder.Description = "In which channel would you want to see the news?";

            await _message.ModifyAsync(x => x.Embed = UpdateBuilder.Build());
            _configurationGate++;
        }

        private async void AddPingRoles(IMessage MessageContext)
        {
            EmbedBuilder UpdateBuilder = _message.Embeds.First().ToEmbedBuilder();

            foreach (ulong Chan in MessageContext.MentionedChannelIds) {
                _mensionChannels.Add(await _configurationGuild.GetChannelAsync(Chan));
            }

            if (_mensionChannels.Count < 1) {
                UpdateBuilder.Description = $"{UpdateBuilder.Description}\n\nPlease have at least one channel to receive the updates";
                await _message.ModifyAsync(x => x.Embed = UpdateBuilder.Build());
                return;
            }

            UpdateBuilder.Description = "Do you want any role to be pinged when a news is added?";

            await _message.ModifyAsync(x => x.Embed = UpdateBuilder.Build());
            _configurationGate++;
        }

        private async void Finish(IMessage MessageContext)
        {
            _mensionRoles = MessageContext.MentionedRoleIds.ToList();
            _mensionRoles.AddRange(MessageContext.MentionedUserIds);

            EmbedBuilder UpdateBuilder = _message.Embeds.First().ToEmbedBuilder();

            UpdateBuilder.Title = "Done";
            UpdateBuilder.Description = "Configuration finished";
            UpdateBuilder.Footer = null;


            await _message.ModifyAsync(x => x.Embed = UpdateBuilder.Build());
            _isConfigFinished = true;
            Globals.ActiveFollows.Add(this);
            Globals.InConfigFollows.Remove(this);
            string GID = (MessageContext.Channel as ITextChannel).GuildId.ToString();
            await Globals.Db.AddFollowToGuild(this, GID);
        }

        private void InitYoutubeInfos(string ChannelUrl)
        {
            JObject YTJson = SplitYTJson(ChannelUrl);
            JArray DataArray = Utilities.Utilities.JsonWalker<JArray>(YTJson, "responseContext/serviceTrackingParams/params");
            string ChannelID = DataArray[0].Value<string>("value");

            RssAdress = _rssBaseAddress + ChannelID;

            string RSSToRead = new WebClient().DownloadString(RssAdress);
            using var Reader = XmlReader.Create(new StringReader(RSSToRead));

            SyndicationFeed feed = SyndicationFeed.Load(Reader);
            var post = feed.Items.FirstOrDefault();

            this.ChannelUrl = ChannelUrl;
            LastKnownVideo = post.Title.Text;
            ChannelName = post.Authors[0].Name;
            ChannelIconUrl = Utilities.Utilities.JsonWalker<string>(YTJson, "metadata/channelMetadataRenderer/avatar/thumbnails/url");
            ChannelDescription = Utilities.Utilities.JsonWalker<string>(YTJson, "metadata/channelMetadataRenderer/description");
        }

        private JObject SplitYTJson(string Link)
        {
            Regex reg = new Regex("ytInitialData = ([^;]+)");
            string YTPage = null;

            using (WebClient wc = new WebClient())
            {
                YTPage = wc.DownloadString(Link);
            }

            if (YTPage == null)
                return null;
            if (reg.Match(YTPage).Success)
                return JsonConvert.DeserializeObject<JObject>(reg.Match(YTPage).Groups[1].Value);

            return null;
        }

        private void SetNewVideoInfos(SyndicationItem Infos, string Flux)
        {
            LastKnownVideo = Infos.Title.Text;
            LastPublicationDate = Infos.LastUpdatedTime.DateTime;
            LastVideoUrl = Infos.Links[0].Uri.ToString();
            LastVideoIconUrl = _thumbnailRegex.Match(Flux).Groups[1].Value;
            LastVideoDescription = _descriptionRegex.Match(Flux).Groups[1].Value;
        }
    }
}
