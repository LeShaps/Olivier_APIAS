using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using APIAS.Data;
using APIAS.Abstracts;

namespace APIAS.Db
{
    class Server
    {
        private string ServerID;
        public string ServerName;
        public List<AFollow> Follows;

        public string id { get => ServerID; set => ServerID = value; }

        public Server() { }

        public Server(JObject jObject)
        {
            Follows = new List<AFollow>();
            ServerID = jObject.Value<string>("id");
            ServerName = jObject.Value<string>("ServerName");
            BuildFollowList(jObject.Value<JArray>("Follows"));
        }

        public void BuildFromJson(JObject jObject)
        {
            ServerID = jObject.Value<string>("id");
            ServerName = jObject.Value<string>("ServerName");
            BuildFollowList(jObject.Value<JArray>("Follows"));
        }

        private void BuildFollowList(JArray jArray)
        {
            foreach (JObject element in jArray)
            {
                dynamic ftype = element["Type"];
                FollowType type = ftype;

                AFollow NewFollow = type switch
                {
                    FollowType.Youtube => element.ToObject<YTFollow>(),
                    _ => throw new NotSupportedException()
                };

                NewFollow.SetMensionChannels(element.Value<JArray>("MentionChannels").ToObject<List<string>>());
                NewFollow.SetMentionRoles(element.Value<JArray>("MentionRoles").ToObject<List<string>>());
                Follows.Add(NewFollow);
            }
        }

        public void InitForUse()
        {
            foreach (AFollow f in Follows) {
                f.InitTimer();
            }
        }
    }
}
