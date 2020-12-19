using System;
using System.Collections.Generic;
using System.Text;

namespace APIAS.Db
{
    class Server
    {
        private string ServerID;
        public string ServerName;
        public List<YTFollow> YTFollows;

        public string id { get => ServerID; set => ServerID = value; }
    }
}
