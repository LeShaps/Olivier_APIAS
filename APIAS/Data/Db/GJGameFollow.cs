using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

using APIAS.Abstracts;
using APIAS.Data;
using APIAS.Utilities;
using APIAS.Extensions;
using Newtonsoft.Json.Linq;

namespace APIAS.Db
{
    class GJGameFollow : AFollow
    {
        /* Game Infos */
        public string Url;
        public string Name;
        public string Developer;
        public Color ThemeColor;

        /* Game update Info */
        public DateTime LastUpdate;
        public string LastVersion;

        /* Abstract implementation */
        public override void CheckUpdate(object obj)
        {
            throw new NotImplementedException();
        }

        public override Task SendUpdateMessage()
        {
            throw new NotImplementedException();
        }

        public GJGameFollow() { }

        public GJGameFollow(ICommandContext Context)
        {
            _configurationGuild = Context.Guild;
            ThemeColor = new Color(3112815);
            SendInitialMessage(Context.Message);
            GuildID = Context.Guild.Id.ToString();

            _gates = new List<ConfigurationUpdater>
            {
                FindGameInfo,
                GameConfirmation,
                UpdateTypes,
                SetupFrequency,
                SetupChannelEnd,
                SetupMensionnedRoles,
                Finish
            };
            Type = FollowType.GameJolt;
        }

        private async void SendInitialMessage(IMessage MessageContext)
        {
            _configUserID = MessageContext.Author.Id;
            _configurationGate = 0;
            _message = await MessageContext.Channel.SendMessageAsync("", false, new EmbedBuilder
            {
                Title = "Setting up now game",
                Description = "Please enter the game's url (name is to come later)",
                Color = ThemeColor,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Configuration made by {MessageContext.Author.Username}"
                }
            }.Build());

            Globals.InConfigFollows.Add(this);
        }

        /* Gates */
        private async void FindGameInfo(IMessage MessageContext)
        {
        }

        private async void GameConfirmation(IMessage MessageContext)
        {
        }

        private async void UpdateTypes(IMessage MessageContext)
        {
        }

        private async void SetupFrequency(IMessage MessageContext)
        {
        }

        private async void SetupChannelEnd(IMessage MessageContext)
        {
        }

        private async void SetupMensionnedRoles(IMessage MessageContext)
        {
        }

        private async void Finish(IMessage MessageContext)
        {
        }

        /* Specific functions */
        private void InitGameInfos(string GameUrl)
        {

        }

        private string ExtractGameVersion(JObject obj)
            => obj.Value<string>("version");
    }
}
