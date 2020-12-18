using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using APIAS.Utilities;
using Discord;
using APIAS.Data;

namespace APIAS
{
    class Program
    {
        public readonly DiscordSocketClient Client;
        public readonly CommandService commands = new CommandService();

        public Program()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            Client.Log += Loggers.LogEventAsync;
        }

        static async Task Main()
        {
            try
            {
                await new Program().MainAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached)
                    throw;
                Console.WriteLine(e.ToString());
            }

        }

        public async Task MainAsync()
        {
            await Loggers.LogEventAsync(new LogMessage(LogSeverity.Info, "Initialisation...", "Stating APIAS")).ConfigureAwait(false);

            Globals.InitConfig();

            await Loggers.LogEventAsync(new LogMessage(LogSeverity.Info, "Setup", "Initializing Modules...")).ConfigureAwait(false);

            await commands.AddModuleAsync<CommunicationModule>(null);

            Client.MessageReceived += HandleMessageAsync;

            commands.Log += Loggers.LogEventAsync;

            await Client.LoginAsync(TokenType.Bot, Globals.BotToken);
            await Client.StartAsync();

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            if (arg.Author.Id == Client.CurrentUser.Id || arg.Author.IsBot)
                return;

            if (!(arg is SocketUserMessage msg))
                return;
            int pos = 0;
            if (msg.HasMentionPrefix(Client.CurrentUser, ref pos) || msg.HasStringPrefix("ap.", ref pos))
            {
                var context = new SocketCommandContext(Client, msg);
                await commands.ExecuteAsync(context, pos, null);
            }
        }
    }
}
