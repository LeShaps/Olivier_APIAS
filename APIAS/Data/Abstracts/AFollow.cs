using Discord;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using APIAS.Data;

namespace APIAS.Abstracts
{
    abstract class AFollow
    {
        /* Configuration by message */
        protected ulong _configUserID;
        protected int _configurationGate;
        protected List<ConfigurationUpdater> _gates;
        protected bool _isConfigFinished;
        protected IUserMessage _message;
        protected IReaction _pendingReaction;
        protected IGuild _configurationGuild;

        /* Properties */
        public IReaction PendingReaction { set => _pendingReaction = value; }
        public List<string> MentionChannels { get => GetMentionChannels(); set => SetMensionChannels(value); }
        public List<string> MentionRoles { get => GetMentionRoles(); set => SetMentionRoles(value); }

        /* Behaviour */
        public FollowType Type;
        public int RefreshTime;
        public string GuildID;
        protected Timer _updateRoutineTimer;
        protected List<ulong> _mensionRoles = new List<ulong>();
        protected List<IGuildChannel> _mensionChannels = new List<IGuildChannel>();

        /* Getters & Setters */
        public ulong ConfigUserID()
            => _configUserID;

        public ulong ConfigMessageID()
            => _message.Id;

        public bool IsFinished()
            => _isConfigFinished;

        private List<string> GetMentionChannels()
        {
            List<string> ChannelsIDs = new List<string>();

            foreach (IGuildChannel chan in _mensionChannels)
            {
                ChannelsIDs.Add(chan.Id.ToString());
            }

            return ChannelsIDs;
        }

        private List<string> GetMentionRoles()
        {
            List<string> RolesIDs = new List<string>();

            foreach (ulong role in _mensionRoles)
            {
                RolesIDs.Add(role.ToString());
            }

            return RolesIDs;
        }

        public async void SetMensionChannels(List<string> Channels)
        {
            Console.WriteLine("Enter channel add");
            IGuild guild = Globals.Client.GetGuild(ulong.Parse(GuildID));
            foreach (string chanId in Channels)
            {
                _mensionChannels.Add(await guild.GetChannelAsync(ulong.Parse(chanId)));
            }
        }

        public void SetMentionRoles(List<string> Roles)
        {
            Console.WriteLine("Enter roles add");
            foreach (string role in Roles)
            {
                _mensionRoles.Add(ulong.Parse(role));
            }
        }

        public void InitTimer()
        {
            _updateRoutineTimer = new Timer(new TimerCallback(CheckUpdate), null, 0, RefreshTime);
        }

        /// <summary>
        /// Used to go the next step of the configuration
        /// </summary>
        /// <param name="ConfigMessage"></param>
        /// <returns></returns>
        public async Task UseNextGate(IMessage ConfigMessage)
        {
            _gates[_configurationGate](ConfigMessage);
        }

        /// <summary>
        /// Cancel current configuration and remove it from the configuration list
        /// </summary>
        public async Task CancelConfiguration()
        {
            _isConfigFinished = true;
            Globals.InConfigFollows.Remove(this);

            EmbedBuilder CancelEmbed = new EmbedBuilder
            {
                Title = "Configuration canceled",
                Color = Color.DarkRed,
                Description = "Configuration canceled by user"
            };

            await _message.ModifyAsync(x => x.Embed = CancelEmbed.Build());
        }

        /// <summary>
        /// Specific update checker
        /// </summary>
        /// <returns></returns>
        public abstract void CheckUpdate(object? obj);

        /// <summary>
        /// Send message to notify update
        /// </summary>
        /// <returns></returns>
        public abstract Task SendUpdateMessage();
    }
}
