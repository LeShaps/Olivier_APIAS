using Discord;
using Discord.Commands;
using Discord.WebSocket;

using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

using APIAS.Data;
using APIAS.Utilities;
using APIAS.Abstracts;

namespace APIAS
{
    class Program
    {
        // public readonly DiscordSocketClient Client;
        public readonly CommandService commands = new CommandService();

        public Program()
        {
            Globals.Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            Globals.Client.Log += Loggers.LogEventAsync;
        }

        static async Task Main()
        {
            try
            {
                await new Program().MainAsync().ConfigureAwait(false);
            }
            catch
            {
                if (Debugger.IsAttached)
                    throw;
            }

        }

        public async Task MainAsync()
        {
            await Loggers.LogEventAsync(new LogMessage(LogSeverity.Info, "Initialisation...", "Stating APIAS")).ConfigureAwait(false);

            Globals.InitConfig();
            await Globals.Db.InitAsync();

            await Loggers.LogEventAsync(new LogMessage(LogSeverity.Info, "Setup", "Initializing Modules...")).ConfigureAwait(false);

            await commands.AddModuleAsync<CommunicationModule>(null);
            await commands.AddModuleAsync<YTFollowModule>(null);

            Globals.Client.MessageReceived += HandleMessageAsync;
            Globals.Client.MessageReceived += CheckConfigUpdate;
            Globals.Client.ReactionAdded += CheckConfigReaction;
            Globals.Client.JoinedGuild += InitGuildAsync;
            Globals.Client.GuildAvailable += InitGuildAsync;

            commands.Log += Loggers.LogEventAsync;

            await Globals.Client.LoginAsync(TokenType.Bot, Globals.BotToken);
            await Globals.Client.StartAsync();

            Globals.CurrentBot = Globals.Client.CurrentUser;

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private async Task CheckConfigReaction(Cacheable<IUserMessage, ulong> Message, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            IUserMessage Mess = await Message.GetOrDownloadAsync();

            AFollow FollowUpdate = Globals.InConfigFollows.Where(x => x.ConfigMessageID() == Mess.Id
                                                                  && Reaction.UserId == x.ConfigUserID()).FirstOrDefault();
            if (FollowUpdate == null) return;

            FollowUpdate.PendingReaction = Reaction;

            await FollowUpdate.UseNextGate(Mess);
            if (FollowUpdate.IsFinished())
                Globals.InConfigFollows.Remove(FollowUpdate);
        }

        private async Task CheckConfigUpdate(SocketMessage arg)
        {
            AFollow FollowUpdate = Globals.InConfigFollows.Where(x => x.ConfigUserID() == arg.Author.Id).FirstOrDefault();
            if (FollowUpdate == null) return;

            await FollowUpdate.UseNextGate(arg);
            if (FollowUpdate.IsFinished())
                Globals.InConfigFollows.Remove(FollowUpdate);
        }

        private async Task InitGuildAsync(SocketGuild arg)
        {
            await Globals.Db.InitGuildAsync(arg);
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            if (arg.Author.Id == Globals.Client.CurrentUser.Id || arg.Author.IsBot)
                return;

            if (!(arg is SocketUserMessage msg))
                return;
            int pos = 0;
            if (msg.HasMentionPrefix(Globals.Client.CurrentUser, ref pos) || msg.HasStringPrefix("ap.", ref pos))
            {
                var context = new SocketCommandContext(Globals.Client, msg);
                await commands.ExecuteAsync(context, pos, null);
            }
        }
    }
}
