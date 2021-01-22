using Discord;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using APIAS.Data;
using System.Linq;
using APIAS.Extensions;

namespace APIAS.Abstracts
{
    abstract class AFollow : IEquatable<AFollow>
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
        public string CreatorUserID { get => _configUserID.ToString(); set => _configUserID = ulong.Parse(value); }

        /* Behaviour */
        public FollowType Type;
        public int RefreshTime;
        public string GuildID;
        public string FollowPublicName;
        public string FollowDbName;
        protected Timer _updateRoutineTimer;
        protected List<ulong> _mensionRoles = new List<ulong>();
        protected List<ITextChannel> _mensionChannels = new List<ITextChannel>();

        /* Getters & Setters */
        public ulong ConfigUserID()
            => _configUserID;

        public ulong ConfigMessageID()
            => _message.Id;

        public bool IsFinished()
            => _isConfigFinished;

        private List<string> GetMentionChannels()
            => _mensionChannels.Select(x => x.Id.ToString()).ToList();

        private List<string> GetMentionRoles()
            => _mensionRoles.Select(x => x.ToString()).ToList();

        public void SetMensionChannels(List<string> Channels)
        {
            IGuild guild = Globals.Client.GetGuild(ulong.Parse(GuildID));
            _mensionChannels.AddRangeUnique(Channels.Select(async x => await guild.GetTextChannelAsync(ulong.Parse(x))).Select(x => x.Result));
        }

        public void SetMentionRoles(List<string> Roles)
        {
            _mensionRoles.AddRangeUnique(Roles.Select(x => ulong.Parse(x)));
        }

        /// <summary>
        /// Initialize the timer
        /// </summary>
        public void InitTimer()
        {
            _isConfigFinished = true;
            _updateRoutineTimer = new Timer(new TimerCallback(CheckUpdate), null, 0, RefreshTime);
        }

        /// <summary>
        /// Used to go the next step of the configuration
        /// </summary>
        /// <param name="ConfigMessage"></param>
        /// <returns></returns>
        public void UseNextGate(IMessage ConfigMessage)
        {
            _gates[_configurationGate](ConfigMessage);
        }

        /// <summary>
        /// Cancel current configuration and remove it from the configuration list
        /// </summary>
        public async Task CancelConfigurationAsync()
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

        public async Task RemoveSubscriptionAsync()
        {
            Globals.ActiveFollows.Remove(this);
            await Globals.Db.RemoveFollowAsync(this, GuildID);
        }

        /// <summary>
        /// Specific update checker
        /// </summary>
        /// <returns></returns>
        public abstract void CheckUpdate(object obj);

        /// <summary>
        /// Send message to notify update
        /// </summary>
        /// <returns></returns>
        public abstract Task SendUpdateMessage();

        /* Implementation of the IEquatable interface */
        bool IEquatable<AFollow>.Equals(AFollow other)
        {
            return FollowDbName == other.FollowDbName;
        }
    }
}
