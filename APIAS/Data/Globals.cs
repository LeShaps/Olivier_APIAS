using Discord;
using Discord.WebSocket;

using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using APIAS.Db;
using APIAS.Abstracts;

namespace APIAS.Data
{
    class Globals
    {
        public static string BotToken { get; private set; }

        public static DiscordSocketClient Client;

        public static DbSystem Db = new DbSystem();

        public static List<AFollow> InConfigFollows = new List<AFollow>();
        public static List<AFollow> ActiveFollows = new List<AFollow>();

        public static string DebugWebhookUrl { get; private set; }

        public static void InitConfig()
        {
            if (!File.Exists("Loggers/Credentials.json"))
                throw new FileNotFoundException($"You must have a \"Credentials.json\" file located in {AppDomain.CurrentDomain.BaseDirectory}Loggers");
            JObject ConfigurationJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("Loggers/Credentials.json"));
            if (ConfigurationJson["botToken"] == null || ConfigurationJson["ownerId"] == null || ConfigurationJson["ownerStr"] == null)
                throw new FileNotFoundException("Missing critical informations in Credentials.json, please complete mandatory informations before continuing");

            BotToken = ConfigurationJson.Value<string>("botToken");
#if DEBUG
            DebugWebhookUrl = ConfigurationJson.Value<string>("DebugWebhookUrl");
#endif
        }
    }

    delegate void ConfigurationUpdater(IMessage Message);

    [Serializable]
    public enum FollowType
    {
        Youtube,
        Itch,
        GameJolt
    }
}
