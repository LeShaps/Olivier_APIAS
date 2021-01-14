using Discord;
using Discord.Commands;

using System;
using System.Threading.Tasks;

namespace APIAS.Utilities
{
    class Loggers
    {
        public async static Task LogEventAsync(LogMessage msg)
        {
            await LogAsync(msg);

            if (msg.Exception is CommandException ex)
            {
                await ex.Context.Channel.SendMessageAsync("", false, new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = msg.Exception.GetType().ToString(),
                    Description = $"An error occured: \n{msg.Exception.InnerException.Message}"
                }.Build()).ConfigureAwait(false);
            }
        }

        private static Task LogAsync(LogMessage msg)
        {
            var cc = Console.ForegroundColor;
            Console.ForegroundColor = msg.Severity switch
            {
                LogSeverity.Critical => ConsoleColor.DarkRed,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Warning => ConsoleColor.DarkYellow,
                LogSeverity.Info => ConsoleColor.White,
                LogSeverity.Verbose => ConsoleColor.Green,
                LogSeverity.Debug => ConsoleColor.DarkGray,
                _ => ConsoleColor.White,
            };

            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {msg}");
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }
    }
}
