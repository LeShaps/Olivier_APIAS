using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using Discord;

namespace APIAS.Utilities
{
    class Utilities
    {
        public static T JsonWalker<T>(JObject OriginalJson, string Path)
        {
            string[] SplitedPath = Path.Split('/');
            string ValueName = SplitedPath.Last();
            JObject FinalJson = OriginalJson;

            foreach (string Part in SplitedPath.SkipLast(1))
            {
                if (FinalJson[Part].Type == JTokenType.Array) {
                    FinalJson = FinalJson.Value<JArray>(Part)[0].ToObject<JObject>();
                } else {
                    FinalJson = FinalJson[Part].ToObject<JObject>();
                }
            }

            return FinalJson.Value<T>(ValueName);
        }

        public static Emoji[] MakeEmojiArray(params string[] Emojis)
        {
            return Emojis.Select(x => new Emoji(x)).ToArray();
        }
    }
}
